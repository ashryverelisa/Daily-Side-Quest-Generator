using System.ComponentModel.DataAnnotations;

namespace DailySideQuestGenerator.Models;

public class QuestTemplateFormModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Base XP is required")]
    [Range(1, 100, ErrorMessage = "Base XP must be between 1 and 100")]
    public int BaseXp { get; set; } = 5;
    
    [Required(ErrorMessage = "Category is required")]
    public string Category { get; set; } = "general";
    
    [Required(ErrorMessage = "Rarity weight is required")]
    [Range(1, 5, ErrorMessage = "Rarity weight must be between 1 and 5")]
    public int RarityWeight { get; set; } = 1;
    
    public bool IsActive { get; set; } = true;
}