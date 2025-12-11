using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services;
using DailySideQuestGenerator.Services.Interfaces;
using NSubstitute;

namespace DailySideQuestGenerator.Tests.Services;

public class QuestServiceTests
{
    private readonly IQuestTemplateService _questTemplateService;
    private readonly IUserProgressService _userProgressService;
    private readonly IDailyQuestService _dailyQuestService;
    private readonly QuestService _questService;

    public QuestServiceTests()
    {
        _questTemplateService = Substitute.For<IQuestTemplateService>();
        _userProgressService = Substitute.For<IUserProgressService>();
        _dailyQuestService = Substitute.For<IDailyQuestService>();
        _questService = new QuestService(_questTemplateService, _userProgressService, _dailyQuestService);
    }
    
    [Fact]
    public async Task InitializeIfNeededAsync_LoadsAllServices()
    {
        // Act
        await _questService.InitializeIfNeededAsync();

        // Assert
        await _userProgressService.Received(1).LoadAsync();
        await _questTemplateService.Received(1).LoadQuestTemplatesAsync();
        await _dailyQuestService.Received(1).LoadAsync();
    }

    [Fact]
    public async Task InitializeIfNeededAsync_WhenCalledTwice_OnlyInitializesOnce()
    {
        // Act
        await _questService.InitializeIfNeededAsync();
        await _questService.InitializeIfNeededAsync();

        // Assert
        await _userProgressService.Received(1).LoadAsync();
        await _questTemplateService.Received(1).LoadQuestTemplatesAsync();
        await _dailyQuestService.Received(1).LoadAsync();
    }

    [Fact]
    public async Task GetTodayQuestsAsync_InitializesBeforeReturning()
    {
        // Arrange
        _dailyQuestService.DailyQuests.Returns([]);

        // Act
        await _questService.GetTodaysQuestsAsync();

        // Assert
        await _userProgressService.Received(1).LoadAsync();
        await _questTemplateService.Received(1).LoadQuestTemplatesAsync();
        await _dailyQuestService.Received(1).LoadAsync();
    }

    [Fact]
    public async Task GetTodayQuestsAsync_ReturnsDailyQuestsFromService()
    {
        // Arrange
        var quests = new List<DailyQuest>
        {
            new() { Id = Guid.NewGuid(), Title = "Quest 1", DateGenerated = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Quest 2", DateGenerated = DateTime.UtcNow }
        };
        _dailyQuestService.DailyQuests.Returns(quests);

        // Act
        var result = await _questService.GetTodaysQuestsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, q => q.Title == "Quest 1");
        Assert.Contains(result, q => q.Title == "Quest 2");
    }

    [Fact]
    public async Task GetTodayQuestsAsync_WhenCalledTwice_ReturnsCachedQuests()
    {
        // Arrange
        var quests = new List<DailyQuest>
        {
            new() { Id = Guid.NewGuid(), Title = "Quest 1", DateGenerated = DateTime.UtcNow }
        };
        _dailyQuestService.DailyQuests.Returns(quests);

        // Act
        var result1 = await _questService.GetTodaysQuestsAsync();
        var result2 = await _questService.GetTodaysQuestsAsync();

        // Assert
        Assert.Same(result1[0], result2[0]);
    }

    [Fact]
    public async Task GetTodayQuestsAsync_ReturnsReadOnlyList()
    {
        // Arrange
        var quests = new List<DailyQuest>
        {
            new() { Id = Guid.NewGuid(), Title = "Quest 1", DateGenerated = DateTime.UtcNow }
        };
        _dailyQuestService.DailyQuests.Returns(quests);

        // Act
        var result = await _questService.GetTodaysQuestsAsync();

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<DailyQuest>>(result);
    }

    [Fact]
    public async Task GetTodayQuestsAsync_WhenNoQuests_ReturnsEmptyList()
    {
        // Arrange
        _dailyQuestService.DailyQuests.Returns([]);

        // Act
        var result = await _questService.GetTodaysQuestsAsync();

        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task ToggleCompleteAsync_WhenQuestNotCompleted_MarksAsCompleted()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var quest = new DailyQuest { Id = questId, Title = "Test Quest", Xp = 10, IsCompleted = false, DateGenerated = DateTime.UtcNow };
        _dailyQuestService.DailyQuests.Returns([quest]);
        await _questService.GetTodaysQuestsAsync(); // Initialize and cache the quest

        // Act
        var result = await _questService.ToggleCompleteAsync(questId);

        // Assert
        Assert.True(result.IsCompleted);
    }

    [Fact]
    public async Task ToggleCompleteAsync_WhenQuestCompleted_MarksAsNotCompleted()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var quest = new DailyQuest { Id = questId, Title = "Test Quest", Xp = 10, IsCompleted = true, DateGenerated = DateTime.UtcNow };
        _dailyQuestService.DailyQuests.Returns([quest]);
        await _questService.GetTodaysQuestsAsync();

        // Act
        var result = await _questService.ToggleCompleteAsync(questId);

        // Assert
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public async Task ToggleCompleteAsync_WhenCompleting_AddsXp()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var quest = new DailyQuest { Id = questId, Title = "Test Quest", Xp = 15, IsCompleted = false, DateGenerated = DateTime.UtcNow };
        _dailyQuestService.DailyQuests.Returns([quest]);
        await _questService.GetTodaysQuestsAsync();

        // Act
        await _questService.ToggleCompleteAsync(questId);

        // Assert
        await _userProgressService.Received(1).AddXpAsync(15);
    }

    [Fact]
    public async Task ToggleCompleteAsync_WhenCompleting_UpdatesStreak()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var quest = new DailyQuest { Id = questId, Title = "Test Quest", Xp = 10, IsCompleted = false, DateGenerated = DateTime.UtcNow };
        _dailyQuestService.DailyQuests.Returns([quest]);
        await _questService.GetTodaysQuestsAsync();

        // Act
        await _questService.ToggleCompleteAsync(questId);

        // Assert
        await _userProgressService.Received(1).UpdateStreakAsync();
    }

    [Fact]
    public async Task ToggleCompleteAsync_WhenUncompleted_RemovesXp()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var quest = new DailyQuest { Id = questId, Title = "Test Quest", Xp = 20, IsCompleted = true, DateGenerated = DateTime.UtcNow };
        _dailyQuestService.DailyQuests.Returns([quest]);
        await _questService.GetTodaysQuestsAsync();

        // Act
        await _questService.ToggleCompleteAsync(questId);

        // Assert
        await _userProgressService.Received(1).RemoveXpAsync(20);
    }

    [Fact]
    public async Task ToggleCompleteAsync_WhenUncompleted_DecrementsStreak()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var quest = new DailyQuest { Id = questId, Title = "Test Quest", Xp = 10, IsCompleted = true, DateGenerated = DateTime.UtcNow };
        _dailyQuestService.DailyQuests.Returns([quest]);
        await _questService.GetTodaysQuestsAsync();

        // Act
        await _questService.ToggleCompleteAsync(questId);

        // Assert
        await _userProgressService.Received(1).DecrementStreakAsync();
    }

    [Fact]
    public async Task ToggleCompleteAsync_SavesAfterToggle()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var quest = new DailyQuest { Id = questId, Title = "Test Quest", Xp = 10, IsCompleted = false, DateGenerated = DateTime.UtcNow };
        _dailyQuestService.DailyQuests.Returns([quest]);
        await _questService.GetTodaysQuestsAsync();

        // Act
        await _questService.ToggleCompleteAsync(questId);

        // Assert
        await _dailyQuestService.Received(1).SaveAsync();
    }

    [Fact]
    public async Task ToggleCompleteAsync_WhenQuestNotFound_ThrowsArgumentException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _dailyQuestService.DailyQuests.Returns([]);
        await _questService.GetTodaysQuestsAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _questService.ToggleCompleteAsync(nonExistentId));
        Assert.Equal("Quest not found", exception.Message);
    }

    [Fact]
    public async Task ToggleCompleteAsync_ReturnsUpdatedQuest()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var quest = new DailyQuest { Id = questId, Title = "Test Quest", Xp = 10, IsCompleted = false, DateGenerated = DateTime.UtcNow };
        _dailyQuestService.DailyQuests.Returns([quest]);
        await _questService.GetTodaysQuestsAsync();

        // Act
        var result = await _questService.ToggleCompleteAsync(questId);

        // Assert
        Assert.Equal(questId, result.Id);
        Assert.Equal("Test Quest", result.Title);
        Assert.Equal(10, result.Xp);
    }

    [Fact]
    public async Task ToggleCompleteAsync_InitializesIfNeeded()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var quest = new DailyQuest { Id = questId, Title = "Test Quest", Xp = 10, IsCompleted = false, DateGenerated = DateTime.UtcNow };
        _dailyQuestService.DailyQuests.Returns([quest]);

        // Act - call toggle directly without calling GetTodaysQuestsAsync first
        // This will initialize and then fail because quest won't be in _generated yet
        await Assert.ThrowsAsync<ArgumentException>(() => _questService.ToggleCompleteAsync(questId));

        // Assert - initialization should still have happened
        await _userProgressService.Received(1).LoadAsync();
        await _questTemplateService.Received(1).LoadQuestTemplatesAsync();
        await _dailyQuestService.Received(1).LoadAsync();
    }

    [Fact]
    public async Task ToggleCompleteAsync_WhenCompletingDoesNotCallRemoveXp()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var quest = new DailyQuest { Id = questId, Title = "Test Quest", Xp = 10, IsCompleted = false, DateGenerated = DateTime.UtcNow };
        _dailyQuestService.DailyQuests.Returns([quest]);
        await _questService.GetTodaysQuestsAsync();

        // Act
        await _questService.ToggleCompleteAsync(questId);

        // Assert
        await _userProgressService.DidNotReceive().RemoveXpAsync(Arg.Any<int>());
        await _userProgressService.DidNotReceive().DecrementStreakAsync();
    }

    [Fact]
    public async Task ToggleCompleteAsync_WhenUncompletedDoesNotCallAddXp()
    {
        // Arrange
        var questId = Guid.NewGuid();
        var quest = new DailyQuest { Id = questId, Title = "Test Quest", Xp = 10, IsCompleted = true, DateGenerated = DateTime.UtcNow };
        _dailyQuestService.DailyQuests.Returns([quest]);
        await _questService.GetTodaysQuestsAsync();

        // Act
        await _questService.ToggleCompleteAsync(questId);

        // Assert
        await _userProgressService.DidNotReceive().AddXpAsync(Arg.Any<int>());
        await _userProgressService.DidNotReceive().UpdateStreakAsync();
    }
}