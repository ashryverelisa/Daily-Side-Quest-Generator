namespace DailySideQuestGenerator.Models;

public class UserProgress
{
    public int TotalXp { get; set; } = 0; 
    public int Level { get; set; } = 1;
    public int DailyStreak { get; set; } = 0;
    public DateTime LastQuestCompleted { get; set; }
}