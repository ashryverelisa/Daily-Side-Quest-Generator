namespace DailySideQuestGenerator.Models;

/// <summary>
/// Contains information about the user's current level and XP progress
/// </summary>
public class LevelInfo
{
    public int Level { get; set; }
    public int TotalXp { get; set; }
    public int XpForCurrentLevel { get; set; }
    public int XpForNextLevel { get; set; }
    public int XpProgressInLevel { get; set; }
    public int XpNeededForNextLevel { get; set; }
    public double ProgressPercentage { get; set; }
    public int DailyStreak { get; set; }
    public int StreakMultiplier { get; set; }
    
    /// <summary>
    /// Gets a title based on the player's level
    /// </summary>
    public string Title => Level switch
    {
        < 5 => "Novice Adventurer",
        < 10 => "Apprentice Quester",
        < 20 => "Journeyman Hero",
        < 35 => "Seasoned Champion",
        < 50 => "Veteran Warrior",
        < 75 => "Elite Guardian",
        < 100 => "Master Legend",
        _ => "Mythic Paragon"
    };
}

