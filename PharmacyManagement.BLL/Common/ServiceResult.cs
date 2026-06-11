namespace PharmacyManagement.BLL.Common;

public class ServiceResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, string[]> Errors { get; init; } = new();

    public static ServiceResult Ok() => new() { Success = true };
    public static ServiceResult Fail(string message) => new() { Success = false, ErrorMessage = message };
    public static ServiceResult ValidationFail(Dictionary<string, string[]> errors) =>
        new() { Success = false, ErrorMessage = "Validation failed.", Errors = errors };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };
    public new static ServiceResult<T> Fail(string message) => new() { Success = false, ErrorMessage = message };

    public static ServiceResult<T> FromValidation(ServiceResult validation) =>
        new() { Success = false, ErrorMessage = validation.ErrorMessage, Errors = validation.Errors };
}
