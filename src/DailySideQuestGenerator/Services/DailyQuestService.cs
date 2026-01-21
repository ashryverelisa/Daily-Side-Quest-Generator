using Blazored.LocalStorage;
using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;

namespace DailySideQuestGenerator.Services;

public class DailyQuestService(ILocalStorageService localStorageService, ICategoryService categoryService, IQuestTemplateService questTemplateService) : IDailyQuestService
{
    public List<DailyQuest> DailyQuests { get; set; } = [];
    private const string StorageKey = "dailyQuests";
    private readonly Random _rng = new();
    private List<QuestTemplate> _templates = [];

    public async Task SaveAsync() => await localStorageService.SetItemAsync(StorageKey, DailyQuests);

    public async Task LoadAsync()
    {
        var loaded = await localStorageService.GetItemAsync<List<DailyQuest>>(StorageKey);
        if (loaded is not null && loaded.Any(x => x.DateGenerated.Date == DateTime.Now.Date))
        {
            DailyQuests = loaded;
        }
        else
        {
            DailyQuests = await GenerateDailyQuests(DateTime.Now.Date);
            await SaveAsync();
        }
    }
    
    private async Task<List<DailyQuest>> GenerateDailyQuests(DateTime forDate)
    {
        // choose 3-5 quests
        var count = _rng.Next(3, 6);

        await categoryService.LoadCategoriesAsync();
        
        var enabledCategories = ( categoryService.GetEnabledCategories()).ToHashSet();
        _templates = questTemplateService.GetQuestTemplates();
        
        var activeTemplates = _templates
            .Where(t => t.IsActive && enabledCategories.Contains(t.Category))
            .ToList();

        // Optional: exclude templates used yesterday
        var yesterday = forDate.AddDays(-1);
        var usedYesterdayTemplateIds = DailyQuests
            .Where(d => d.DateGenerated.Date == yesterday)
            .Select(d => d.TemplateId)
            .ToHashSet();

        var pool = activeTemplates.Where(t => !usedYesterdayTemplateIds.Contains(t.Id)).ToList();
        if (pool.Count == 0) pool = activeTemplates; // fallback

        // weighted pick by RarityWeight (lower weight means more common if you prefer),
        // here we'll treat higher weight -> higher chance.
        var selections = new List<DailyQuest>();
        var attempts = 0;

        while (selections.Count < count && attempts < 50)
        {
            attempts++;
            var chosen = WeightedPick(pool);
            if (chosen == null) break;

            // avoid duplicates in same day's picks
            if (selections.Any(s => s.TemplateId == chosen.Id)) continue;

            selections.Add(new DailyQuest
            {
                DateGenerated = forDate,
                TemplateId = chosen.Id,
                Title = chosen.Title,
                Xp = chosen.BaseXp,
                Category = chosen.Category,
                IsCompleted = false
            });
        }

        // Safety: if not enough selected (rare), fallback to random unique picks
        if (selections.Count < count)
        {
            var remaining = pool.Where(p => selections.All(s => s.TemplateId != p.Id)).OrderBy(_ => _rng.Next()).Take(count - selections.Count);

            selections.AddRange(remaining.Select(r => new DailyQuest
            {
                DateGenerated = forDate,
                TemplateId = r.Id,
                Title = r.Title,
                Xp = r.BaseXp,
                Category = r.Category
            }));
        }

        return selections;
    }

    private QuestTemplate? WeightedPick(List<QuestTemplate> pool)
    {
        var total = pool.Sum(t => t.RarityWeight);
        if (total <= 0) return pool.OrderBy(_ => _rng.Next()).FirstOrDefault();

        var pick = _rng.Next(0, total);
        var acc = 0;

        foreach (var t in pool)
        {
            acc += t.RarityWeight;
            if (pick < acc) return t;
        }

        return pool.FirstOrDefault();
    }
}