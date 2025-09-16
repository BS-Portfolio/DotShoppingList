using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.DTOs.Post;

public record ShoppingListPost(
    [MaxLength(50)]
    [Required]
    string ShoppingListName);