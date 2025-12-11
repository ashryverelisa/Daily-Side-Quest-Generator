namespace DailySideQuestGenerator.Models;

/// </summary>
/// Represents an XP gain event with details for UI feedback
/// <summary>
public class XpEvent
{
    public double ProgressPercentage { get; set; }
    public int XpProgressInLevel { get; set; }
    public int XpToNextLevel { get; set; }
    public bool LeveledUp => NewLevel > PreviousLevel;
    public int PreviousLevel { get; set; }
    public int NewLevel { get; set; }
    public int TotalXp { get; set; }
    public int StreakBonus { get; set; }
    public int XpGained { get; set; }
}


