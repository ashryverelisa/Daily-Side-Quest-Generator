using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;

namespace DailySideQuestGenerator.Services;

public class QuestService(ICategoryService categoryService, IQuestTemplateService questTemplateService) : IQuestService
{
    private List<QuestTemplate> _templates = [];
    private readonly List<DailyQuest> _generated = [];
    private readonly UserProgress _progress = new();
    private readonly Random _rng = new();
    private bool _initialized = false;

    public async Task<IReadOnlyList<DailyQuest>> GetTodaysQuestsAsync()
    {
        await InitializeIfNeededAsync();

        var today = DateTime.UtcNow.Date;
        var existing = _generated.Where(q => q.DateGenerated.Date == today).ToList();
        if (existing.Count != 0) return existing;

        var generated = GenerateDailyQuests(today);
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
            _progress.TotalXp += q.Xp;
            _progress.Level = CalculateLevel(_progress.TotalXp);
            // naive streak: increment if not already incremented today and at least one completed
            // For Milestone 1 keep it simple: increment streak whenever a quest completed (demo).
            _progress.DailyStreak += 1;
        }
        else
        {
            // undo XP on uncheck (simple behavior)
            _progress.TotalXp = Math.Max(0, _progress.TotalXp - q.Xp);
            _progress.Level = CalculateLevel(_progress.TotalXp);
            _progress.DailyStreak = Math.Max(0, _progress.DailyStreak - 1);
        }

        return q;
    }

    public async Task InitializeIfNeededAsync()
    {
        if (_initialized) return;
        await questTemplateService.LoadQuestTemplatesAsync();
        _initialized = true;
        _templates = questTemplateService.GetQuestTemplates();
    }

    public Task<UserProgress> GetProgressAsync()
    {
        return Task.FromResult(_progress);
    }
    
    private int CalculateLevel(int totalXp)
    {
        // Example formula: level grows roughly as sqrt(totalXp/10)
        return Math.Max(1, (int)Math.Floor(Math.Sqrt(totalXp / 10.0)) + 1);
    }

    private List<DailyQuest> GenerateDailyQuests(DateTime forDate)
    {
        // choose 3-5 quests
        var count = _rng.Next(3, 6);
        
        var enabledCategories = ( categoryService.GetEnabledCategoriesAsync()).ToHashSet();

        var activeTemplates = _templates
            .Where(t => t.IsActive && enabledCategories.Contains(t.Category))
            .ToList();
        
        // Optional: exclude templates used yesterday
        var yesterday = forDate.AddDays(-1);
        var usedYesterdayTemplateIds = _generated
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