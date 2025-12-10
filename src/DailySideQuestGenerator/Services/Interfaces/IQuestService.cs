using DailySideQuestGenerator.Models;

namespace DailySideQuestGenerator.Services.Interfaces;

public interface IQuestService
{
    /// <summary>
    /// Retrieves the list of daily quests for the current day.
    /// If quests have not yet been generated for today, it will generate a new set.
    /// </summary>
    /// <returns>A read-only list of <see cref="DailyQuest"/> objects for today.</returns>
    Task<IReadOnlyList<DailyQuest>> GetTodaysQuestsAsync();
    
    /// <summary>
    /// Toggles the completion state of a specific daily quest.
    /// If the quest is marked as completed, XP and streak will be updated accordingly.
    /// </summary>
    /// <param name="dailyQuestId">The unique identifier of the daily quest to toggle.</param>
    /// <returns>The updated <see cref="DailyQuest"/> after toggling completion.</returns>
    Task<DailyQuest> ToggleCompleteAsync(Guid dailyQuestId);

    /// <summary>
    /// Initializes quest templates and any internal state if needed.
    /// Should be called before generating or retrieving quests to ensure templates are available.
    /// </summary>
    Task InitializeIfNeededAsync();
    
    UserProgress UserProgress { get; set; }
}