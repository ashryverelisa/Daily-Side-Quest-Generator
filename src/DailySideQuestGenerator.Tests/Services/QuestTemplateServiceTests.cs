using Blazored.LocalStorage;
using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services;
using NSubstitute;

namespace DailySideQuestGenerator.Tests.Services;

public class QuestTemplateServiceTests
{
    private readonly ILocalStorageService _localStorageService;
    private readonly QuestTemplateService _questTemplateService;

    public QuestTemplateServiceTests()
    {
        _localStorageService = Substitute.For<ILocalStorageService>();
        _questTemplateService = new QuestTemplateService(_localStorageService);
    }

    [Fact]
    public void GetQuestTemplates_WhenNotLoaded_ReturnsEmptyList()
    {
        // Act
        var result = _questTemplateService.GetQuestTemplates();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetQuestTemplates_AfterLoad_ReturnsSameListInstance()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);
        await _questTemplateService.LoadQuestTemplatesAsync();

        // Act
        var result1 = _questTemplateService.GetQuestTemplates();
        var result2 = _questTemplateService.GetQuestTemplates();

        // Assert
        Assert.Same(result1, result2);
    }
    
    [Fact]
    public async Task LoadQuestTemplatesAsync_WhenNoStoredTemplates_SeedsDefaultTemplates()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);

        // Act
        await _questTemplateService.LoadQuestTemplatesAsync();
        var result = _questTemplateService.GetQuestTemplates();

        // Assert
        Assert.Equal(10, result.Count);
    }

    [Fact]
    public async Task LoadQuestTemplatesAsync_WhenNoStoredTemplates_SeedsCorrectTemplates()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);

        // Act
        await _questTemplateService.LoadQuestTemplatesAsync();
        var result = _questTemplateService.GetQuestTemplates();

        // Assert
        Assert.Contains(result, t => t is { Title: "Drink 1L of water", BaseXp: 5, Category: "health" });
        Assert.Contains(result, t => t is { Title: "Clean your desk", BaseXp: 8, Category: "chores" });
        Assert.Contains(result, t => t is { Title: "Watch 1 new anime episode", BaseXp: 3, Category: "fun" });
        Assert.Contains(result, t => t is { Title: "Stretch for 5 minutes", BaseXp: 4, Category: "health" });
        Assert.Contains(result, t => t is { Title: "Read 10 pages of a book", BaseXp: 6, Category: "learning" });
        Assert.Contains(result, t => t is { Title: "Send a message to a friend", BaseXp: 6, Category: "social" });
        Assert.Contains(result, t => t is { Title: "Tidy one shelf", BaseXp: 7, Category: "chores" });
        Assert.Contains(result, t => t is { Title: "Try a 5-minute breathing exercise", BaseXp: 4, Category: "health" });
        Assert.Contains(result, t => t is { Title: "Sketch something for 10 minutes", BaseXp: 5, Category: "creative" });
        Assert.Contains(result, t => t is { Title: "Plan tomorrow for 5 minutes", BaseXp: 4, Category: "productivity" });
    }

    [Fact]
    public async Task LoadQuestTemplatesAsync_WhenNoStoredTemplates_SeedsCorrectRarityWeights()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);

        // Act
        await _questTemplateService.LoadQuestTemplatesAsync();
        var result = _questTemplateService.GetQuestTemplates();

        // Assert
        Assert.Contains(result, t => t is { Title: "Drink 1L of water", RarityWeight: 1 });
        Assert.Contains(result, t => t is { Title: "Clean your desk", RarityWeight: 2 });
        Assert.Contains(result, t => t is { Title: "Watch 1 new anime episode", RarityWeight: 3 });
        Assert.Contains(result, t => t is { Title: "Sketch something for 10 minutes", RarityWeight: 4 });
    }

    [Fact]
    public async Task LoadQuestTemplatesAsync_WhenNoStoredTemplates_PersistsToStorage()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);

        // Act
        await _questTemplateService.LoadQuestTemplatesAsync();

        // Assert
        await _localStorageService.Received(1).SetItemAsync(
            "questTemplates",
            Arg.Is<List<QuestTemplate>>(list => list.Count == 10));
    }

    [Fact]
    public async Task LoadQuestTemplatesAsync_WhenStoredTemplatesExist_LoadsFromStorage()
    {
        // Arrange
        var storedTemplates = new List<QuestTemplate>
        {
            new() { Title = "Custom Quest 1", BaseXp = 15, Category = "custom", RarityWeight = 2 },
            new() { Title = "Custom Quest 2", BaseXp = 20, Category = "custom", RarityWeight = 3 }
        };
        _localStorageService.ContainKeyAsync("questTemplates").Returns(true);
        _localStorageService.GetItemAsync<List<QuestTemplate>>("questTemplates")
            .Returns(storedTemplates);

        // Act
        await _questTemplateService.LoadQuestTemplatesAsync();
        var result = _questTemplateService.GetQuestTemplates();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t is { Title: "Custom Quest 1", BaseXp: 15 });
        Assert.Contains(result, t => t is { Title: "Custom Quest 2", BaseXp: 20 });
    }

    [Fact]
    public async Task LoadQuestTemplatesAsync_WhenStoredTemplatesExist_DoesNotPersist()
    {
        // Arrange
        var storedTemplates = new List<QuestTemplate>
        {
            new() { Title = "Custom Quest 1", BaseXp = 15, Category = "custom" }
        };
        _localStorageService.ContainKeyAsync("questTemplates").Returns(true);
        _localStorageService.GetItemAsync<List<QuestTemplate>>("questTemplates")
            .Returns(storedTemplates);

        // Act
        await _questTemplateService.LoadQuestTemplatesAsync();

        // Assert
        await _localStorageService.DidNotReceive().SetItemAsync(
            Arg.Any<string>(),
            Arg.Any<List<QuestTemplate>>());
    }

    [Fact]
    public async Task LoadQuestTemplatesAsync_WhenStoredTemplatesExist_PreservesAllProperties()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var storedTemplates = new List<QuestTemplate>
        {
            new()
            {
                Id = templateId,
                Title = "Test Quest",
                Description = "Test Description",
                BaseXp = 25,
                Category = "test",
                RarityWeight = 5,
                IsActive = false
            }
        };
        _localStorageService.ContainKeyAsync("questTemplates").Returns(true);
        _localStorageService.GetItemAsync<List<QuestTemplate>>("questTemplates")
            .Returns(storedTemplates);

        // Act
        await _questTemplateService.LoadQuestTemplatesAsync();
        var result = _questTemplateService.GetQuestTemplates();

        // Assert
        var template = Assert.Single(result);
        Assert.Equal(templateId, template.Id);
        Assert.Equal("Test Quest", template.Title);
        Assert.Equal("Test Description", template.Description);
        Assert.Equal(25, template.BaseXp);
        Assert.Equal("test", template.Category);
        Assert.Equal(5, template.RarityWeight);
        Assert.False(template.IsActive);
    }

    [Fact]
    public async Task LoadQuestTemplatesAsync_SeededTemplates_HaveDefaultIsActiveTrue()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);

        // Act
        await _questTemplateService.LoadQuestTemplatesAsync();
        var result = _questTemplateService.GetQuestTemplates();

        // Assert
        Assert.All(result, t => Assert.True(t.IsActive));
    }

    [Fact]
    public async Task LoadQuestTemplatesAsync_SeededTemplates_HaveUniqueIds()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);

        // Act
        await _questTemplateService.LoadQuestTemplatesAsync();
        var result = _questTemplateService.GetQuestTemplates();

        // Assert
        var ids = result.Select(t => t.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public async Task LoadQuestTemplatesAsync_SeededTemplates_CoverAllExpectedCategories()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);

        // Act
        await _questTemplateService.LoadQuestTemplatesAsync();
        var result = _questTemplateService.GetQuestTemplates();

        // Assert
        var categories = result.Select(t => t.Category).Distinct().ToList();
        Assert.Contains("health", categories);
        Assert.Contains("chores", categories);
        Assert.Contains("fun", categories);
        Assert.Contains("learning", categories);
        Assert.Contains("social", categories);
        Assert.Contains("creative", categories);
        Assert.Contains("productivity", categories);
    }
}