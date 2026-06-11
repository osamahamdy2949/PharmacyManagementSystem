using FluentValidation;
using PharmacyManagement.BLL.Common;

namespace PharmacyManagement.BLL.Validators;

internal static class ValidationHelper
{
    public static async Task<ServiceResult?> ValidateAsync<T>(IValidator<T> validator, T model)
    {
        var result = await validator.ValidateAsync(model);
        if (result.IsValid) return null;

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return ServiceResult.ValidationFail(errors);
    }
}
