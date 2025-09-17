using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Model.DTOs.Post;

public record ShoppingListPostDto(
    [MaxLength(50)]
    [Required]
    string ShoppingListName);