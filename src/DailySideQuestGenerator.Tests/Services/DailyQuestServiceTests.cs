using Blazored.LocalStorage;
using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services;
using DailySideQuestGenerator.Services.Interfaces;
using NSubstitute;

namespace DailySideQuestGenerator.Tests.Services;

public class DailyQuestServiceTests
{
    private readonly ILocalStorageService _localStorageService;
    private readonly ICategoryService _categoryService;
    private readonly IQuestTemplateService _questTemplateService;
    private readonly DailyQuestService _dailyQuestService;

    public DailyQuestServiceTests()
    {
        _localStorageService = Substitute.For<ILocalStorageService>();
        _categoryService = Substitute.For<ICategoryService>();
        _questTemplateService = Substitute.For<IQuestTemplateService>();
        _dailyQuestService = new DailyQuestService(_localStorageService, _categoryService, _questTemplateService);
    }

    [Fact]
    public async Task SaveAsync_PersistsDailyQuestsToStorage()
    {
        // Arrange
        _dailyQuestService.DailyQuests = [new DailyQuest { Title = "Test Quest", Xp = 10 }];

        // Act
        await _dailyQuestService.SaveAsync();

        // Assert
        await _localStorageService.Received(1).SetItemAsync("dailyQuests", _dailyQuestService.DailyQuests);
    }

    [Fact]
    public async Task SaveAsync_PersistsEmptyList()
    {
        // Arrange
        _dailyQuestService.DailyQuests = [];

        // Act
        await _dailyQuestService.SaveAsync();

        // Assert
        await _localStorageService.Received(1).SetItemAsync("dailyQuests", _dailyQuestService.DailyQuests);
    }

    [Fact]
    public async Task LoadAsync_WhenStoredQuestsExistForToday_LoadsFromStorage()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var storedQuests = new List<DailyQuest>
        {
            new() { Title = "Stored Quest 1", Xp = 10, DateGenerated = today },
            new() { Title = "Stored Quest 2", Xp = 15, DateGenerated = today }
        };
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns(storedQuests);

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        Assert.Equal(2, _dailyQuestService.DailyQuests.Count);
        Assert.Contains(_dailyQuestService.DailyQuests, q => q.Title == "Stored Quest 1");
        Assert.Contains(_dailyQuestService.DailyQuests, q => q.Title == "Stored Quest 2");
    }

    [Fact]
    public async Task LoadAsync_WhenStoredQuestsExistForToday_DoesNotGenerateNew()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var storedQuests = new List<DailyQuest>
        {
            new() { Title = "Stored Quest", Xp = 10, DateGenerated = today }
        };
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns(storedQuests);

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        await _localStorageService.DidNotReceive().SetItemAsync(
            Arg.Any<string>(),
            Arg.Any<List<DailyQuest>>());
    }

    [Fact]
    public async Task LoadAsync_WhenNoStoredQuests_GeneratesNewQuests()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns((List<DailyQuest>?)null);
        
        SetupDefaultTemplatesAndCategories();

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        Assert.NotEmpty(_dailyQuestService.DailyQuests);
        Assert.InRange(_dailyQuestService.DailyQuests.Count, 3, 5);
    }

    [Fact]
    public async Task LoadAsync_WhenStoredQuestsAreOld_GeneratesNewQuests()
    {
        // Arrange
        var yesterday = DateTime.Now.Date.AddDays(-1);
        var storedQuests = new List<DailyQuest>
        {
            new() { Title = "Old Quest", Xp = 10, DateGenerated = yesterday }
        };
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns(storedQuests);
        
        SetupDefaultTemplatesAndCategories();

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        Assert.All(_dailyQuestService.DailyQuests, q => Assert.Equal(DateTime.Now.Date, q.DateGenerated.Date));
    }

    [Fact]
    public async Task LoadAsync_WhenGeneratingNewQuests_SavesToStorage()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns((List<DailyQuest>?)null);
        
        SetupDefaultTemplatesAndCategories();

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        await _localStorageService.Received(1).SetItemAsync("dailyQuests", _dailyQuestService.DailyQuests);
    }

    [Fact]
    public async Task LoadAsync_GeneratedQuests_HaveCorrectDate()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns((List<DailyQuest>?)null);
        
        SetupDefaultTemplatesAndCategories();

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        Assert.All(_dailyQuestService.DailyQuests, q => Assert.Equal(DateTime.Now.Date, q.DateGenerated.Date));
    }

    [Fact]
    public async Task LoadAsync_GeneratedQuests_AreNotCompleted()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns((List<DailyQuest>?)null);
        
        SetupDefaultTemplatesAndCategories();

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        Assert.All(_dailyQuestService.DailyQuests, q => Assert.False(q.IsCompleted));
    }

    [Fact]
    public async Task LoadAsync_GeneratedQuests_HaveUniqueTemplateIds()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns((List<DailyQuest>?)null);
        
        SetupDefaultTemplatesAndCategories();

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        var templateIds = _dailyQuestService.DailyQuests.Select(q => q.TemplateId).ToList();
        Assert.Equal(templateIds.Count, templateIds.Distinct().Count());
    }

    [Fact]
    public async Task LoadAsync_GeneratedQuests_OnlyUseEnabledCategories()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns((List<DailyQuest>?)null);
        
        var enabledCategories = new List<string> { "health", "fun" };
        _categoryService.GetEnabledCategoriesAsync().Returns(enabledCategories);
        
        var templates = new List<QuestTemplate>
        {
            new() { Id = Guid.NewGuid(), Title = "Health Quest 1", Category = "health", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Health Quest 2", Category = "health", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Fun Quest 1", Category = "fun", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Fun Quest 2", Category = "fun", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Fun Quest 3", Category = "fun", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Disabled Category Quest", Category = "chores", RarityWeight = 1, IsActive = true }
        };
        _questTemplateService.GetQuestTemplates().Returns(templates);

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        Assert.All(_dailyQuestService.DailyQuests, q => Assert.Contains(q.Category, enabledCategories));
    }

    [Fact]
    public async Task LoadAsync_GeneratedQuests_OnlyUseActiveTemplates()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns((List<DailyQuest>?)null);
        
        var enabledCategories = new List<string> { "health" };
        _categoryService.GetEnabledCategoriesAsync().Returns(enabledCategories);
        
        var inactiveTemplateId = Guid.NewGuid();
        var templates = new List<QuestTemplate>
        {
            new() { Id = Guid.NewGuid(), Title = "Active Quest 1", Category = "health", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Active Quest 2", Category = "health", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Active Quest 3", Category = "health", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Active Quest 4", Category = "health", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Active Quest 5", Category = "health", RarityWeight = 1, IsActive = true },
            new() { Id = inactiveTemplateId, Title = "Inactive Quest", Category = "health", RarityWeight = 1, IsActive = false }
        };
        _questTemplateService.GetQuestTemplates().Returns(templates);

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        Assert.DoesNotContain(_dailyQuestService.DailyQuests, q => q.TemplateId == inactiveTemplateId);
    }

    [Fact]
    public async Task LoadAsync_GeneratedQuests_CopyPropertiesFromTemplate()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns((List<DailyQuest>?)null);
        
        var enabledCategories = new List<string> { "health" };
        _categoryService.GetEnabledCategoriesAsync().Returns(enabledCategories);
        
        var templateId = Guid.NewGuid();
        var templates = new List<QuestTemplate>
        {
            new() { Id = templateId, Title = "Specific Quest", Category = "health", BaseXp = 25, RarityWeight = 100, IsActive = true }
        };
        _questTemplateService.GetQuestTemplates().Returns(templates);

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        var quest = _dailyQuestService.DailyQuests.First();
        Assert.Equal(templateId, quest.TemplateId);
        Assert.Equal("Specific Quest", quest.Title);
        Assert.Equal("health", quest.Category);
        Assert.Equal(25, quest.Xp);
    }

    [Fact]
    public async Task LoadAsync_WhenNoTemplatesAvailable_GeneratesEmptyList()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns((List<DailyQuest>?)null);
        
        _categoryService.GetEnabledCategoriesAsync().Returns(new List<string> { "health" });
        _questTemplateService.GetQuestTemplates().Returns([]);

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        Assert.Empty(_dailyQuestService.DailyQuests);
    }

    [Fact]
    public async Task LoadAsync_LoadsCategoriesBeforeGenerating()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns((List<DailyQuest>?)null);
        
        SetupDefaultTemplatesAndCategories();

        // Act
        await _dailyQuestService.LoadAsync();

        // Assert
        await _categoryService.Received(1).LoadCategoriesAsync();
    }

    [Fact]
    public async Task LoadAsync_GeneratesCorrectNumberOfQuests()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns((List<DailyQuest>?)null);
        
        SetupDefaultTemplatesAndCategories();

        // Act - run multiple times to test the range
        var questCounts = new List<int>();
        for (var i = 0; i < 20; i++)
        {
            _dailyQuestService.DailyQuests = []; // Reset
            await _dailyQuestService.LoadAsync();
            questCounts.Add(_dailyQuestService.DailyQuests.Count);
        }

        // Assert - all counts should be between 3 and 5
        Assert.All(questCounts, count => Assert.InRange(count, 3, 5));
    }

    [Fact]
    public void DailyQuests_InitiallyEmpty()
    {
        // Assert
        Assert.Empty(_dailyQuestService.DailyQuests);
    }

    [Fact]
    public void DailyQuests_CanBeSet()
    {
        // Arrange
        var quests = new List<DailyQuest>
        {
            new() { Title = "Quest 1" },
            new() { Title = "Quest 2" }
        };

        // Act
        _dailyQuestService.DailyQuests = quests;

        // Assert
        Assert.Equal(2, _dailyQuestService.DailyQuests.Count);
    }
    
    [Fact]
    public async Task LoadAsync_HigherRarityWeight_MoreLikelyToBePicked()
    {
        // Arrange
        _localStorageService.GetItemAsync<List<DailyQuest>>("dailyQuests")
            .Returns((List<DailyQuest>?)null);
        
        var enabledCategories = new List<string> { "test" };
        _categoryService.GetEnabledCategoriesAsync().Returns(enabledCategories);
        
        var highWeightId = Guid.NewGuid();
        var lowWeightId = Guid.NewGuid();
        var templates = new List<QuestTemplate>
        {
            new() { Id = highWeightId, Title = "High Weight Quest", Category = "test", RarityWeight = 100, IsActive = true },
            new() { Id = lowWeightId, Title = "Low Weight Quest", Category = "test", RarityWeight = 1, IsActive = true }
        };
        _questTemplateService.GetQuestTemplates().Returns(templates);

        // Act - run multiple times
        var highWeightCount = 0;
        const int totalRuns = 100;
        
        for (var i = 0; i < totalRuns; i++)
        {
            _dailyQuestService.DailyQuests = [];
            await _dailyQuestService.LoadAsync();
            
            // Check if high weight quest was picked first (more likely due to higher weight)
            if (_dailyQuestService.DailyQuests.First().TemplateId == highWeightId)
                highWeightCount++;
        }

        // Assert - high weight should be picked significantly more often (statistically)
        // With weights 100:1, high weight should be picked ~99% of the time
        Assert.True(highWeightCount > 80, $"High weight quest was only picked {highWeightCount} times out of {totalRuns}");
    }

    private void SetupDefaultTemplatesAndCategories()
    {
        var enabledCategories = new List<string> { "health", "chores", "fun" };
        _categoryService.GetEnabledCategoriesAsync().Returns(enabledCategories);
        
        var templates = new List<QuestTemplate>
        {
            new() { Id = Guid.NewGuid(), Title = "Quest 1", Category = "health", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Quest 2", Category = "health", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Quest 3", Category = "chores", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Quest 4", Category = "chores", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Quest 5", Category = "fun", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Quest 6", Category = "fun", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Quest 7", Category = "health", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Quest 8", Category = "chores", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Quest 9", Category = "fun", RarityWeight = 1, IsActive = true },
            new() { Id = Guid.NewGuid(), Title = "Quest 10", Category = "health", RarityWeight = 1, IsActive = true }
        };
        _questTemplateService.GetQuestTemplates().Returns(templates);
    }
}