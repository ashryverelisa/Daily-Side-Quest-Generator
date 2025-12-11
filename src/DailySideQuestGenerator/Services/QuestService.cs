using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;

namespace DailySideQuestGenerator.Services;

public class QuestService(
    IQuestTemplateService questTemplateService, 
    IUserProgressService userProgressService, 
    IDailyQuestService dailyQuestService,
    IXpService xpService) : IQuestService
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


    public async Task<QuestToggleResult> ToggleCompleteAsync(Guid dailyQuestId)
    {
        await InitializeIfNeededAsync();

        var q = _generated.FirstOrDefault(x => x.Id == dailyQuestId);
        if (q == null) throw new ArgumentException("Quest not found");

        q.IsCompleted = !q.IsCompleted;

        QuestToggleResult result;
        
        if (q.IsCompleted)
        {
            var xpEvent = await xpService.AwardQuestXpAsync(q.Xp);
            result = new QuestToggleResult
            {
                Quest = q,
                XpEvent = xpEvent,
                LevelInfo = xpService.GetLevelInfo(),
                WasCompleted = true
            };
        }
        else
        {
            var levelInfo = await xpService.RemoveQuestXpAsync(q.Xp);
            result = new QuestToggleResult
            {
                Quest = q,
                XpEvent = null,
                LevelInfo = levelInfo,
                WasCompleted = false
            };
        }

        await dailyQuestService.SaveAsync();
        
        return result;
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