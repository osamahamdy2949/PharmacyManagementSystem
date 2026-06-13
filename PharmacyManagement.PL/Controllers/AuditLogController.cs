using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.DAL.Common;
using PharmacyManagement.DAL.Data.DbContexts;
using PharmacyManagement.DAL.Data.Entities;
using System.Text.Json;

namespace PharmacyManagement.PL.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class AuditLogController : Controller
{
    private readonly PharmacyDbContext _context;

    public AuditLogController(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var logs = await _context.AuditLogs
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync();
            
        var userIds = logs.Select(l => l.UserId).Where(u => !string.IsNullOrEmpty(u)).Distinct().ToList();
        var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FullName);

        var viewModels = logs.Select(l => new AuditLogViewModel
        {
            Id = l.Id,
            CreatedAt = l.CreatedAt,
            UserId = l.UserId,
            UserName = !string.IsNullOrEmpty(l.UserId) && users.ContainsKey(l.UserId) ? users[l.UserId] : "System",
            Action = GetDisplayAction(l.Action, l.NewValues),
            Entity = l.Entity,
            EntityId = l.EntityId
        }).ToList();

        return View(viewModels);
    }
    
    public async Task<IActionResult> Details(int id)
    {
        var log = await _context.AuditLogs.FindAsync(id);
        if (log == null) return NotFound();

        var action = GetDisplayAction(log.Action, log.NewValues);
        var changes = await BuildChangesAsync(log.OldValues, log.NewValues, action);
        
        var vm = new AuditLogViewModel
        {
            Id = log.Id,
            CreatedAt = log.CreatedAt,
            UserId = log.UserId,
            UserName = !string.IsNullOrEmpty(log.UserId) ? (await _context.Users.FindAsync(log.UserId))?.FullName ?? "System" : "System",
            Action = action,
            Entity = log.Entity,
            EntityId = log.EntityId,
            OldValues = log.OldValues,
            NewValues = log.NewValues,
            Changes = changes
        };

        return View(vm);
    }

    private static string GetDisplayAction(string action, string? newValues)
    {
        if (action != "Modified" || string.IsNullOrWhiteSpace(newValues))
            return action;

        var values = ParseValues(newValues);
        return values.TryGetValue(nameof(BaseEntity.IsDeleted), out var deleted) &&
               string.Equals(deleted, bool.TrueString, StringComparison.OrdinalIgnoreCase)
            ? "Deleted"
            : action;
    }

    private async Task<IReadOnlyList<AuditLogChangeViewModel>> BuildChangesAsync(string? oldValuesJson, string? newValuesJson, string action)
    {
        var oldValues = ParseValues(oldValuesJson);
        var newValues = ParseValues(newValuesJson);
        var userNames = await LoadUserNamesAsync(oldValues.Values.Concat(newValues.Values));
        var keys = oldValues.Keys.Union(newValues.Keys)
            .Where(ShouldShowField)
            .OrderBy(k => k)
            .ToList();

        var changes = new List<AuditLogChangeViewModel>();
        foreach (var key in keys)
        {
            oldValues.TryGetValue(key, out var oldValue);
            newValues.TryGetValue(key, out var newValue);

            if (action == "Modified" && string.Equals(oldValue, newValue, StringComparison.Ordinal))
                continue;

            changes.Add(new AuditLogChangeViewModel
            {
                Field = ToDisplayName(key),
                OldValue = FormatValue(key, oldValue, userNames),
                NewValue = FormatValue(key, newValue, userNames)
            });
        }

        return changes;
    }

    private async Task<Dictionary<string, string>> LoadUserNamesAsync(IEnumerable<string?> values)
    {
        var ids = values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!)
            .Distinct()
            .ToList();

        return await _context.Users
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.FullName) ? u.UserName ?? u.Email ?? u.Id : u.FullName);
    }

    private static Dictionary<string, string> ParseValues(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)?
                .ToDictionary(pair => pair.Key, pair => JsonElementToString(pair.Value))
                ?? new Dictionary<string, string>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>();
        }
    }

    private static string JsonElementToString(JsonElement value) =>
        value.ValueKind switch
        {
            JsonValueKind.Null => string.Empty,
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            _ => value.ToString()
        };

    private static bool ShouldShowField(string field) =>
        field is not nameof(BaseEntity.Id)
            and not nameof(BaseEntity.IsDeleted)
            and not nameof(BaseEntity.CreatedBy)
            and not nameof(BaseEntity.UpdatedBy)
            and not nameof(BaseEntity.DeletedBy);

    private static string FormatValue(string field, string? value, IReadOnlyDictionary<string, string> userNames)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "-";

        if (IsUserField(field) && userNames.TryGetValue(value, out var userName))
            return userName;

        return DateTime.TryParse(value, out var date)
            ? DateDisplayHelper.FormatShortDateTime(date)
            : value;
    }

    private static bool IsUserField(string field) =>
        field.EndsWith("UserId", StringComparison.OrdinalIgnoreCase)
        || field.Equals("UserId", StringComparison.OrdinalIgnoreCase);

    private static string ToDisplayName(string field)
    {
        var chars = new List<char>();
        for (var i = 0; i < field.Length; i++)
        {
            if (i > 0 && char.IsUpper(field[i]) && !char.IsUpper(field[i - 1]))
                chars.Add(' ');

            chars.Add(field[i]);
        }

        return new string(chars.ToArray());
    }
}

public class AuditLogViewModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public IReadOnlyList<AuditLogChangeViewModel> Changes { get; set; } = Array.Empty<AuditLogChangeViewModel>();
}

public class AuditLogChangeViewModel
{
    public string Field { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
}
