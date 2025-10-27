using System.ComponentModel.DataAnnotations;

namespace PrevisionalAccountManager.Models.DataBaseEntities;

public class CategoryModel
{
    [Key]
    public int Id { get; init; }
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public UserModel? User { get; init; }
    public int OwnerUserId { get; set; }
}