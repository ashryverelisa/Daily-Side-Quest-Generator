namespace DailySideQuestGenerator.Models;

public class QuestTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int BaseXP { get; set; } = 5;
    public string Category { get; set; } = "general";
    /// <summary>1..5 higher means less common if you invert, or use directly for weighted picks</summary>
    public int RarityWeight { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}