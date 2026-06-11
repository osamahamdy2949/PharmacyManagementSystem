using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PharmacyManagement.BLL.ViewModels.CategoryViewModels
{
    public class CreateCategoryViewModel
    {
        [Required, StringLength(50)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }
    }
}
