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

    [Fact]
    public async Task GetQuestTemplateById_WhenExists_ReturnsTemplate()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var storedTemplates = new List<QuestTemplate>
        {
            new() { Id = templateId, Title = "Test Quest", BaseXp = 10, Category = "test" }
        };
        _localStorageService.ContainKeyAsync("questTemplates").Returns(true);
        _localStorageService.GetItemAsync<List<QuestTemplate>>("questTemplates").Returns(storedTemplates);
        await _questTemplateService.LoadQuestTemplatesAsync();

        // Act
        var result = _questTemplateService.GetQuestTemplateById(templateId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Quest", result.Title);
    }

    [Fact]
    public async Task GetQuestTemplateById_WhenNotExists_ReturnsNull()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);
        await _questTemplateService.LoadQuestTemplatesAsync();

        // Act
        var result = _questTemplateService.GetQuestTemplateById(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddQuestTemplateAsync_AddsTemplateToList()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);
        await _questTemplateService.LoadQuestTemplatesAsync();
        var initialCount = _questTemplateService.GetQuestTemplates().Count;
        var newTemplate = new QuestTemplate { Title = "New Quest", BaseXp = 15, Category = "new" };

        // Act
        await _questTemplateService.AddQuestTemplateAsync(newTemplate);

        // Assert
        var templates = _questTemplateService.GetQuestTemplates();
        Assert.Equal(initialCount + 1, templates.Count);
        Assert.Contains(templates, t => t.Title == "New Quest");
    }

    [Fact]
    public async Task AddQuestTemplateAsync_AssignsNewGuid()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);
        await _questTemplateService.LoadQuestTemplatesAsync();
        var newTemplate = new QuestTemplate { Id = Guid.Empty, Title = "New Quest", BaseXp = 15, Category = "new" };

        // Act
        await _questTemplateService.AddQuestTemplateAsync(newTemplate);

        // Assert
        var addedTemplate = _questTemplateService.GetQuestTemplates().First(t => t.Title == "New Quest");
        Assert.NotEqual(Guid.Empty, addedTemplate.Id);
    }

    [Fact]
    public async Task AddQuestTemplateAsync_PersistsToStorage()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);
        await _questTemplateService.LoadQuestTemplatesAsync();
        _localStorageService.ClearReceivedCalls();
        var newTemplate = new QuestTemplate { Title = "New Quest", BaseXp = 15, Category = "new" };

        // Act
        await _questTemplateService.AddQuestTemplateAsync(newTemplate);

        // Assert
        await _localStorageService.Received(1).SetItemAsync(
            "questTemplates",
            Arg.Is<List<QuestTemplate>>(list => list.Any(t => t.Title == "New Quest")));
    }
    
    [Fact]
    public async Task UpdateQuestTemplateAsync_UpdatesExistingTemplate()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var storedTemplates = new List<QuestTemplate>
        {
            new() { Id = templateId, Title = "Original Title", BaseXp = 10, Category = "test" }
        };
        _localStorageService.ContainKeyAsync("questTemplates").Returns(true);
        _localStorageService.GetItemAsync<List<QuestTemplate>>("questTemplates").Returns(storedTemplates);
        await _questTemplateService.LoadQuestTemplatesAsync();

        var updatedTemplate = new QuestTemplate
        {
            Id = templateId,
            Title = "Updated Title",
            Description = "New Description",
            BaseXp = 20,
            Category = "updated",
            RarityWeight = 3,
            IsActive = false
        };

        // Act
        await _questTemplateService.UpdateQuestTemplateAsync(updatedTemplate);

        // Assert
        var template = _questTemplateService.GetQuestTemplateById(templateId);
        Assert.NotNull(template);
        Assert.Equal("Updated Title", template.Title);
        Assert.Equal("New Description", template.Description);
        Assert.Equal(20, template.BaseXp);
        Assert.Equal("updated", template.Category);
        Assert.Equal(3, template.RarityWeight);
        Assert.False(template.IsActive);
    }

    [Fact]
    public async Task UpdateQuestTemplateAsync_WhenNotExists_DoesNothing()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);
        await _questTemplateService.LoadQuestTemplatesAsync();
        var initialCount = _questTemplateService.GetQuestTemplates().Count;
        _localStorageService.ClearReceivedCalls();

        var nonExistentTemplate = new QuestTemplate
        {
            Id = Guid.NewGuid(),
            Title = "Non-existent",
            BaseXp = 10,
            Category = "test"
        };

        // Act
        await _questTemplateService.UpdateQuestTemplateAsync(nonExistentTemplate);

        // Assert
        Assert.Equal(initialCount, _questTemplateService.GetQuestTemplates().Count);
        await _localStorageService.DidNotReceive().SetItemAsync(
            Arg.Any<string>(),
            Arg.Any<List<QuestTemplate>>());
    }

    [Fact]
    public async Task UpdateQuestTemplateAsync_PersistsToStorage()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var storedTemplates = new List<QuestTemplate>
        {
            new() { Id = templateId, Title = "Original Title", BaseXp = 10, Category = "test" }
        };
        _localStorageService.ContainKeyAsync("questTemplates").Returns(true);
        _localStorageService.GetItemAsync<List<QuestTemplate>>("questTemplates").Returns(storedTemplates);
        await _questTemplateService.LoadQuestTemplatesAsync();
        _localStorageService.ClearReceivedCalls();

        var updatedTemplate = new QuestTemplate { Id = templateId, Title = "Updated Title", BaseXp = 20, Category = "test" };

        // Act
        await _questTemplateService.UpdateQuestTemplateAsync(updatedTemplate);

        // Assert
        await _localStorageService.Received(1).SetItemAsync(
            "questTemplates",
            Arg.Is<List<QuestTemplate>>(list => list.Any(t => t.Title == "Updated Title")));
    }
    
    [Fact]
    public async Task DeleteQuestTemplateAsync_RemovesTemplateFromList()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var storedTemplates = new List<QuestTemplate>
        {
            new() { Id = templateId, Title = "To Delete", BaseXp = 10, Category = "test" },
            new() { Id = Guid.NewGuid(), Title = "Keep This", BaseXp = 15, Category = "test" }
        };
        _localStorageService.ContainKeyAsync("questTemplates").Returns(true);
        _localStorageService.GetItemAsync<List<QuestTemplate>>("questTemplates").Returns(storedTemplates);
        await _questTemplateService.LoadQuestTemplatesAsync();

        // Act
        await _questTemplateService.DeleteQuestTemplateAsync(templateId);

        // Assert
        var templates = _questTemplateService.GetQuestTemplates();
        Assert.Single(templates);
        Assert.DoesNotContain(templates, t => t.Title == "To Delete");
        Assert.Contains(templates, t => t.Title == "Keep This");
    }

    [Fact]
    public async Task DeleteQuestTemplateAsync_WhenNotExists_DoesNothing()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);
        await _questTemplateService.LoadQuestTemplatesAsync();
        var initialCount = _questTemplateService.GetQuestTemplates().Count;
        _localStorageService.ClearReceivedCalls();

        // Act
        await _questTemplateService.DeleteQuestTemplateAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(initialCount, _questTemplateService.GetQuestTemplates().Count);
        await _localStorageService.DidNotReceive().SetItemAsync(
            Arg.Any<string>(),
            Arg.Any<List<QuestTemplate>>());
    }

    [Fact]
    public async Task DeleteQuestTemplateAsync_PersistsToStorage()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var storedTemplates = new List<QuestTemplate>
        {
            new() { Id = templateId, Title = "To Delete", BaseXp = 10, Category = "test" }
        };
        _localStorageService.ContainKeyAsync("questTemplates").Returns(true);
        _localStorageService.GetItemAsync<List<QuestTemplate>>("questTemplates").Returns(storedTemplates);
        await _questTemplateService.LoadQuestTemplatesAsync();
        _localStorageService.ClearReceivedCalls();

        // Act
        await _questTemplateService.DeleteQuestTemplateAsync(templateId);

        // Assert
        await _localStorageService.Received(1).SetItemAsync(
            "questTemplates",
            Arg.Is<List<QuestTemplate>>(list => !list.Any(t => t.Id == templateId)));
    }
    
    [Fact]
    public async Task ToggleActiveAsync_WhenActive_DeactivatesTemplate()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var storedTemplates = new List<QuestTemplate>
        {
            new() { Id = templateId, Title = "Test", BaseXp = 10, Category = "test", IsActive = true }
        };
        _localStorageService.ContainKeyAsync("questTemplates").Returns(true);
        _localStorageService.GetItemAsync<List<QuestTemplate>>("questTemplates").Returns(storedTemplates);
        await _questTemplateService.LoadQuestTemplatesAsync();

        // Act
        await _questTemplateService.ToggleActiveAsync(templateId);

        // Assert
        var template = _questTemplateService.GetQuestTemplateById(templateId);
        Assert.NotNull(template);
        Assert.False(template.IsActive);
    }

    [Fact]
    public async Task ToggleActiveAsync_WhenInactive_ActivatesTemplate()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var storedTemplates = new List<QuestTemplate>
        {
            new() { Id = templateId, Title = "Test", BaseXp = 10, Category = "test", IsActive = false }
        };
        _localStorageService.ContainKeyAsync("questTemplates").Returns(true);
        _localStorageService.GetItemAsync<List<QuestTemplate>>("questTemplates").Returns(storedTemplates);
        await _questTemplateService.LoadQuestTemplatesAsync();

        // Act
        await _questTemplateService.ToggleActiveAsync(templateId);

        // Assert
        var template = _questTemplateService.GetQuestTemplateById(templateId);
        Assert.NotNull(template);
        Assert.True(template.IsActive);
    }

    [Fact]
    public async Task ToggleActiveAsync_WhenNotExists_DoesNothing()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("questTemplates").Returns(false);
        await _questTemplateService.LoadQuestTemplatesAsync();
        _localStorageService.ClearReceivedCalls();

        // Act
        await _questTemplateService.ToggleActiveAsync(Guid.NewGuid());

        // Assert
        await _localStorageService.DidNotReceive().SetItemAsync(
            Arg.Any<string>(),
            Arg.Any<List<QuestTemplate>>());
    }

    [Fact]
    public async Task ToggleActiveAsync_PersistsToStorage()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var storedTemplates = new List<QuestTemplate>
        {
            new() { Id = templateId, Title = "Test", BaseXp = 10, Category = "test", IsActive = true }
        };
        _localStorageService.ContainKeyAsync("questTemplates").Returns(true);
        _localStorageService.GetItemAsync<List<QuestTemplate>>("questTemplates").Returns(storedTemplates);
        await _questTemplateService.LoadQuestTemplatesAsync();
        _localStorageService.ClearReceivedCalls();

        // Act
        await _questTemplateService.ToggleActiveAsync(templateId);

        // Assert
        await _localStorageService.Received(1).SetItemAsync(
            "questTemplates",
            Arg.Is<List<QuestTemplate>>(list => list.Any(t => t.Id == templateId && !t.IsActive)));
    }
}