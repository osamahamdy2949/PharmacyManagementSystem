using System.ComponentModel.DataAnnotations;

namespace PharmacyManagement.BLL.ViewModels.CategoryViewModels;

public class CategoryViewModel
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }
}
