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
    /// Retrieves all available quest templates in the system.
    /// These templates serve as the pool for daily quest generation.
    /// </summary>
    /// <returns>A read-only list of <see cref="QuestTemplate"/> objects.</returns>
    Task<IReadOnlyList<QuestTemplate>> GetAllTemplatesAsync();

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

    /// <summary>
    /// Retrieves the current progress of the user, including total XP, level, and daily streak.
    /// </summary>
    /// <returns>A <see cref="UserProgress"/> object representing the user's current progress.</returns>
    Task<UserProgress> GetProgressAsync();
}