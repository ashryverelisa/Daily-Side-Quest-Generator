using DailySideQuestGenerator.Models;

namespace DailySideQuestGenerator.Services.Interfaces;

/// <summary>
/// Service for managing XP, leveling, and streak calculations
/// </summary>
public interface IXpService
{
    /// <summary>
    /// Gets the current level information for the user
    /// </summary>
    LevelInfo GetLevelInfo();
    
    /// <summary>
    /// Calculates the level for a given total XP amount
    /// </summary>
    int CalculateLevel(int totalXp);
    
    /// <summary>
    /// Calculates the total XP required to reach a specific level
    /// </summary>
    int GetXpForLevel(int level);
    
    /// <summary>
    /// Gets the XP bonus multiplier based on the current streak
    /// </summary>
    int GetStreakMultiplier(int streak);
    
    /// <summary>
    /// Calculates the XP with streak bonus applied
    /// </summary>
    int ApplyStreakBonus(int baseXp, int streak);
    
    /// <summary>
    /// Awards XP for completing a quest and returns the XP event details
    /// </summary>
    Task<XpEvent> AwardQuestXpAsync(int baseXp);
    
    /// <summary>
    /// Removes XP when a quest is uncompleted and returns the updated level info
    /// </summary>
    Task<LevelInfo> RemoveQuestXpAsync(int xp);
}

