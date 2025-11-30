using DailySideQuestGenerator.Services;

namespace DailySideQuestGenerator.Tests.Services;

public class QuestServiceTests
{
    [Fact]
    public async Task Initialize_Populates_Templates()
    {
        var svc = new QuestService();

        await svc.InitializeIfNeededAsync();
        var templates = await svc.GetAllTemplatesAsync();

        Assert.NotNull(templates);
        Assert.Equal(10, templates.Count); // seeded templates count
    }

    [Fact]
    public async Task Initialize_Called_Twice_Does_Not_Duplicate_Templates()
    {
        var svc = new QuestService();

        await svc.InitializeIfNeededAsync();
        await svc.InitializeIfNeededAsync();

        var templates = await svc.GetAllTemplatesAsync();
        Assert.Equal(10, templates.Count);
    }

    [Fact]
    public async Task GetProgressAsync_Returns_Zeroed_State_Before_Completion()
    {
        var svc = new QuestService();

        await svc.InitializeIfNeededAsync();
        var progress = await svc.GetProgressAsync();

        Assert.Equal(0, progress.TotalXP);
        Assert.Equal(1, progress.Level);
        Assert.Equal(0, progress.DailyStreak);
    }

    [Fact]
    public async Task ToggleComplete_InvalidQuest_Throws()
    {
        var svc = new QuestService();

        await Assert.ThrowsAsync<ArgumentException>(() => svc.ToggleCompleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetTodaysQuests_Generates_And_Caches()
    {
        var svc = new QuestService();

        var first = (await svc.GetTodaysQuestsAsync()).ToList();
        Assert.InRange(first.Count, 3, 5);

        var second = (await svc.GetTodaysQuestsAsync()).ToList();
        Assert.Equal(first.Count, second.Count);

        Assert.True(first.Select(f => f.TemplateId).SequenceEqual(second.Select(s => s.TemplateId)));
    }

    [Fact]
    public async Task ToggleComplete_Updates_Progress_And_Streak()
    {
        var svc = new QuestService();

        var quests = (await svc.GetTodaysQuestsAsync()).ToList();
        Assert.NotEmpty(quests);

        var q = quests.First();

        var completed = await svc.ToggleCompleteAsync(q.Id);
        Assert.True(completed.IsCompleted);

        var progressAfterComplete = await svc.GetProgressAsync();
        Assert.Equal(q.XP, progressAfterComplete.TotalXP);
        Assert.Equal(1, progressAfterComplete.DailyStreak);

        var undone = await svc.ToggleCompleteAsync(q.Id);
        Assert.False(undone.IsCompleted);

        var progressAfterUndo = await svc.GetProgressAsync();
        Assert.Equal(0, progressAfterUndo.TotalXP);
        Assert.Equal(0, progressAfterUndo.DailyStreak);
    }
}