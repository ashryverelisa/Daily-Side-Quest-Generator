using DailySideQuestGenerator.Services;
using DailySideQuestGenerator.Services.Interfaces;
using NSubstitute;

namespace DailySideQuestGenerator.Tests.Services;

public class QuestServiceTests
{
    private readonly ICategoryService _categoryService = Substitute.For<ICategoryService>();
    private readonly QuestService _questService;
    
    public QuestServiceTests()
    {
        _questService = new QuestService(_categoryService);   
    }
    
    [Fact]
    public async Task Initialize_Populates_Templates()
    {
        await _questService.InitializeIfNeededAsync();
        var templates = await _questService.GetAllTemplatesAsync();

        Assert.NotNull(templates);
        Assert.Equal(10, templates.Count); // seeded templates count
    }

    [Fact]
    public async Task Initialize_Called_Twice_Does_Not_Duplicate_Templates()
    {
        await _questService.InitializeIfNeededAsync();
        await _questService.InitializeIfNeededAsync();

        var templates = await _questService.GetAllTemplatesAsync();
        Assert.Equal(10, templates.Count);
    }

    [Fact]
    public async Task GetProgressAsync_Returns_Zeroed_State_Before_Completion()
    {
        var svc = new QuestService(_categoryService);

        await svc.InitializeIfNeededAsync();
        var progress = await svc.GetProgressAsync();

        Assert.Equal(0, progress.TotalXp);
        Assert.Equal(1, progress.Level);
        Assert.Equal(0, progress.DailyStreak);
    }

    [Fact]
    public async Task ToggleComplete_InvalidQuest_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _questService.ToggleCompleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetTodaysQuests_Generates_And_Caches()
    {
        var first = (await _questService.GetTodaysQuestsAsync()).ToList();
        Assert.InRange(first.Count, 3, 5);

        var second = (await _questService.GetTodaysQuestsAsync()).ToList();
        Assert.Equal(first.Count, second.Count);

        Assert.True(first.Select(f => f.TemplateId).SequenceEqual(second.Select(s => s.TemplateId)));
    }

    [Fact]
    public async Task ToggleComplete_Updates_Progress_And_Streak()
    {

        var quests = (await _questService.GetTodaysQuestsAsync()).ToList();
        Assert.NotEmpty(quests);

        var q = quests.First();

        var completed = await _questService.ToggleCompleteAsync(q.Id);
        Assert.True(completed.IsCompleted);

        var progressAfterComplete = await _questService.GetProgressAsync();
        Assert.Equal(q.Xp, progressAfterComplete.TotalXp);
        Assert.Equal(1, progressAfterComplete.DailyStreak);

        var undone = await _questService.ToggleCompleteAsync(q.Id);
        Assert.False(undone.IsCompleted);

        var progressAfterUndo = await _questService.GetProgressAsync();
        Assert.Equal(0, progressAfterUndo.TotalXp);
        Assert.Equal(0, progressAfterUndo.DailyStreak);
    }
}