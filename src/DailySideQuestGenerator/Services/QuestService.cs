using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;

namespace DailySideQuestGenerator.Services;

public class QuestService(IQuestTemplateService questTemplateService, IUserProgressService userProgressService, IDailyQuestService dailyQuestService) : IQuestService
{
    private readonly List<DailyQuest> _generated = [];
    private bool _initialized;

    public async Task<IReadOnlyList<DailyQuest>> GetTodaysQuestsAsync()
    {
        await InitializeIfNeededAsync();

        var today = DateTime.UtcNow.Date;
        var existing = _generated.Where(q => q.DateGenerated.Date == today).ToList();
        if (existing.Count != 0) return existing;
        
        var generated = dailyQuestService.DailyQuests;
        _generated.AddRange(generated);
        return generated;
    }


    public async Task<DailyQuest> ToggleCompleteAsync(Guid dailyQuestId)
    {
        await InitializeIfNeededAsync();

        var q = _generated.FirstOrDefault(x => x.Id == dailyQuestId);
        if (q == null) throw new ArgumentException("Quest not found");

        q.IsCompleted = !q.IsCompleted;

        if (q.IsCompleted)
        {
            await userProgressService.AddXpAsync(q.Xp);
            await userProgressService.UpdateStreakAsync();
        }
        else
        {
            await userProgressService.RemoveXpAsync(q.Xp);
            await userProgressService.DecrementStreakAsync();
        }

        await dailyQuestService.SaveAsync();
        
        return q;
    }

    public async Task InitializeIfNeededAsync()
    {
        if (_initialized) return;
        await userProgressService.LoadAsync();
        await questTemplateService.LoadQuestTemplatesAsync();
        await dailyQuestService.LoadAsync();
        _initialized = true;
    }
}