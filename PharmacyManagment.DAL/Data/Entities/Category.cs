namespace PharmacyManagement.DAL.Data.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
}
