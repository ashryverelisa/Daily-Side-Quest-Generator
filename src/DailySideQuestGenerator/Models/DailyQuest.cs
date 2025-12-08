namespace DailySideQuestGenerator.Models;

public class DailyQuest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime DateGenerated { get; set; } = DateTime.UtcNow;
    public Guid TemplateId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Xp { get; set; }
    public string Category { get; set; } = "general";
    public bool IsCompleted { get; set; } 
}