using Microsoft.AspNetCore.Mvc.ModelBinding;
using PharmacyManagement.BLL.Common;

namespace PharmacyManagement.PL.Helpers;

public static class ControllerExtensions
{
    public static void AddServiceErrors(this ModelStateDictionary modelState, ServiceResult result)
    {
        if (!string.IsNullOrEmpty(result.ErrorMessage))
            modelState.AddModelError(string.Empty, result.ErrorMessage);

        foreach (var error in result.Errors)
        {
            foreach (var message in error.Value)
                modelState.AddModelError(error.Key, message);
        }
    }
}
