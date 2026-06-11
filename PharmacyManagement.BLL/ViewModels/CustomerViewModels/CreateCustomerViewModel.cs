using PharmacyManagement.BLL.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PharmacyManagement.BLL.ViewModels.CustomerViewModels
{
    public class CreateCustomerViewModel
    {
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(11, MinimumLength = 11)]
        [RegularExpression(ValidationRules.EgyptianPhonePattern, ErrorMessage = ValidationRules.EgyptianPhoneMessage)]
        public string Phone { get; set; } = string.Empty;
    }
}
