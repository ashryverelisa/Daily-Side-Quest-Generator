using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;

namespace DailySideQuestGenerator.Services;

/// <summary>
/// Service for managing XP, leveling, and streak calculations
/// Uses a quadratic leveling formula: XP needed = BaseXp * Level^2
/// This creates a satisfying progression curve where early levels are quick
/// but higher levels require more dedication.
/// </summary>
public class XpService(IUserProgressService userProgressService) : IXpService
{
    // Base XP required per level (scales quadratically)
    private const int BaseXpPerLevel = 100;
    
    // Maximum streak bonus percentage (50% at 7+ day streak)
    private const int MaxStreakBonusPercent = 50;
    
    /// <inheritdoc />
    public LevelInfo GetLevelInfo()
    {
        var progress = userProgressService.Progress;
        var level = CalculateLevel(progress.TotalXp);
        var xpForCurrentLevel = GetXpForLevel(level);
        var xpForNextLevel = GetXpForLevel(level + 1);
        var xpProgressInLevel = progress.TotalXp - xpForCurrentLevel;
        var xpNeededForNextLevel = xpForNextLevel - xpForCurrentLevel;
        
        return new LevelInfo
        {
            Level = level,
            TotalXp = progress.TotalXp,
            XpForCurrentLevel = xpForCurrentLevel,
            XpForNextLevel = xpForNextLevel,
            XpProgressInLevel = xpProgressInLevel,
            XpNeededForNextLevel = xpNeededForNextLevel,
            ProgressPercentage = xpNeededForNextLevel > 0 
                ? Math.Min(100, (double)xpProgressInLevel / xpNeededForNextLevel * 100)
                : 100,
            DailyStreak = progress.DailyStreak,
            StreakMultiplier = GetStreakMultiplier(progress.DailyStreak)
        };
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// Level formula: Level = floor(sqrt(TotalXP / BaseXP)) + 1
    /// This creates a smooth quadratic curve where:
    /// - Level 1: 0 XP
    /// - Level 2: 100 XP
    /// - Level 3: 400 XP
    /// - Level 5: 1,600 XP
    /// - Level 10: 8,100 XP
    /// - Level 20: 36,100 XP
    /// </remarks>
    public int CalculateLevel(int totalXp)
    {
        if (totalXp <= 0) return 1;
        return (int)Math.Floor(Math.Sqrt((double)totalXp / BaseXpPerLevel)) + 1;
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// Inverse of the level formula: XP = BaseXP * (Level - 1)^2
    /// </remarks>
    public int GetXpForLevel(int level)
    {
        if (level <= 1) return 0;
        return BaseXpPerLevel * (level - 1) * (level - 1);
    }
    
    /// <inheritdoc />
    /// <remarks>
    /// Streak multiplier provides bonus XP based on consecutive daily completions:
    /// - 0 days: 0% bonus
    /// - 1 day: 5% bonus  
    /// - 2 days: 10% bonus
    /// - 3 days: 15% bonus
    /// - 4 days: 20% bonus
    /// - 5 days: 30% bonus
    /// - 6 days: 40% bonus
    /// - 7+ days: 50% bonus (max)
    /// </remarks>
    public int GetStreakMultiplier(int streak)
    {
        return streak switch
        {
            0 => 0,
            1 => 5,
            2 => 10,
            3 => 15,
            4 => 20,
            5 => 30,
            6 => 40,
            _ => MaxStreakBonusPercent
        };
    }
    
    /// <inheritdoc />
    public int ApplyStreakBonus(int baseXp, int streak)
    {
        var multiplier = GetStreakMultiplier(streak);
        var bonus = (int)Math.Ceiling(baseXp * multiplier / 100.0);
        return baseXp + bonus;
    }
    
    /// <inheritdoc />
    public async Task<XpEvent> AwardQuestXpAsync(int baseXp)
    {
        var progress = userProgressService.Progress;
        var previousLevel = CalculateLevel(progress.TotalXp);
        
        // Calculate streak bonus
        var streakBonus = ApplyStreakBonus(baseXp, progress.DailyStreak) - baseXp;
        var totalXpGained = baseXp + streakBonus;
        
        // Add XP and update streak
        await userProgressService.AddXpAsync(totalXpGained);
        await userProgressService.UpdateStreakAsync();
        
        // Get updated progress
        var newProgress = userProgressService.Progress;
        var newLevel = CalculateLevel(newProgress.TotalXp);
        var xpForNextLevel = GetXpForLevel(newLevel + 1);
        var xpForCurrentLevel = GetXpForLevel(newLevel);
        var xpProgressInLevel = newProgress.TotalXp - xpForCurrentLevel;
        var xpNeededForNextLevel = xpForNextLevel - xpForCurrentLevel;
        
        return new XpEvent
        {
            XpGained = baseXp,
            StreakBonus = streakBonus,
            TotalXp = newProgress.TotalXp,
            NewLevel = newLevel,
            PreviousLevel = previousLevel,
            XpToNextLevel = xpNeededForNextLevel - xpProgressInLevel,
            XpProgressInLevel = xpProgressInLevel,
            ProgressPercentage = xpNeededForNextLevel > 0
                ? Math.Min(100, (double)xpProgressInLevel / xpNeededForNextLevel * 100)
                : 100
        };
    }
    
    /// <inheritdoc />
    public async Task<LevelInfo> RemoveQuestXpAsync(int xp)
    {
        await userProgressService.RemoveXpAsync(xp);
        await userProgressService.DecrementStreakAsync();
        return GetLevelInfo();
    }
}

