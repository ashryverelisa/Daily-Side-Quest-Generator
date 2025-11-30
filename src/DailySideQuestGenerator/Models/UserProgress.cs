namespace DailySideQuestGenerator.Models;

public class UserProgress
{
    public int TotalXP { get; set; } = 0; 
    public int Level { get; set; } = 1;
    public int DailyStreak { get; set; } = 0;
}