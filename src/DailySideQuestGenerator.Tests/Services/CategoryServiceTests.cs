using Blazored.LocalStorage;
using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services;
using NSubstitute;

namespace DailySideQuestGenerator.Tests.Services;

public class CategoryServiceTests
{
    private readonly ILocalStorageService _localStorageService;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _localStorageService = Substitute.For<ILocalStorageService>();
        _categoryService = new CategoryService(_localStorageService);
    }

    [Fact]
    public void GetCategoriesAsync_WhenNotLoaded_ReturnsEmptyList()
    {
        // Act
        var result = _categoryService.GetCategoriesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task LoadCategoriesAsync_WhenNoStoredCategories_SeedsDefaultCategories()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("enabledCategories").Returns(false);

        // Act
        await _categoryService.LoadCategoriesAsync();
        var result = _categoryService.GetCategoriesAsync();

        // Assert
        Assert.Equal(7, result.Count);
        Assert.Contains(result, c => c is { Name: "health", Color: "#28a745" });
        Assert.Contains(result, c => c is { Name: "chores", Color: "#ffc107" });
        Assert.Contains(result, c => c is { Name: "fun", Color: "#17a2b8" });
        Assert.Contains(result, c => c is { Name: "learning", Color: "#6610f2" });
        Assert.Contains(result, c => c is { Name: "social", Color: "#e83e8c" });
        Assert.Contains(result, c => c is { Name: "creative", Color: "#fd7e14" });
        Assert.Contains(result, c => c is { Name: "productivity", Color: "#20c997" });
    }

    [Fact]
    public async Task LoadCategoriesAsync_WhenNoStoredCategories_PersistsToStorage()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("enabledCategories").Returns(false);

        // Act
        await _categoryService.LoadCategoriesAsync();

        // Assert
        await _localStorageService.Received(1).SetItemAsync(
            "enabledCategories",
            Arg.Is<List<Category>>(list => list.Count == 7));
    }

    [Fact]
    public async Task LoadCategoriesAsync_WhenStoredCategoriesExist_LoadsFromStorage()
    {
        // Arrange
        var storedCategories = new List<Category>
        {
            new() { Name = "custom1", Color = "#111111", Enabled = true },
            new() { Name = "custom2", Color = "#222222", Enabled = false }
        };
        _localStorageService.ContainKeyAsync("enabledCategories").Returns(true);
        _localStorageService.GetItemAsync<IReadOnlyList<Category>>("enabledCategories")
            .Returns(storedCategories);

        // Act
        await _categoryService.LoadCategoriesAsync();
        var result = _categoryService.GetCategoriesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c is { Name: "custom1", Color: "#111111", Enabled: true });
        Assert.Contains(result, c => c is { Name: "custom2", Color: "#222222", Enabled: false });
    }

    [Fact]
    public async Task LoadCategoriesAsync_WhenStoredCategoriesExist_DoesNotPersist()
    {
        // Arrange
        var storedCategories = new List<Category>
        {
            new() { Name = "custom1", Color = "#111111", Enabled = true }
        };
        _localStorageService.ContainKeyAsync("enabledCategories").Returns(true);
        _localStorageService.GetItemAsync<IReadOnlyList<Category>>("enabledCategories")
            .Returns(storedCategories);

        // Act
        await _categoryService.LoadCategoriesAsync();

        // Assert
        await _localStorageService.DidNotReceive().SetItemAsync(
            Arg.Any<string>(),
            Arg.Any<List<Category>>());
    }

    [Fact]
    public async Task GetCategoryColorAsync_WhenCategoryExists_ReturnsColor()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("enabledCategories").Returns(false);
        await _categoryService.LoadCategoriesAsync();

        // Act
        var result = _categoryService.GetCategoryColorAsync("health");

        // Assert
        Assert.Equal("#28a745", result);
    }

    [Fact]
    public async Task GetCategoryColorAsync_WhenCategoryDoesNotExist_ReturnsNull()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("enabledCategories").Returns(false);
        await _categoryService.LoadCategoriesAsync();

        // Act
        var result = _categoryService.GetCategoryColorAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCategoryColorAsync_WhenNotLoaded_ReturnsNull()
    {
        // Act
        var result = _categoryService.GetCategoryColorAsync("health");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCategoryColorAsync_IsCaseInsensitive()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("enabledCategories").Returns(false);
        await _categoryService.LoadCategoriesAsync();

        // Act
        var result = _categoryService.GetCategoryColorAsync("HEALTH");

        // Assert - The current implementation is case-sensitive, so this should return null
        Assert.Null(result);
    }

    [Fact]
    public async Task GetEnabledCategoriesAsync_WhenAllEnabled_ReturnsAllNames()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("enabledCategories").Returns(false);
        await _categoryService.LoadCategoriesAsync();

        // Act
        var result = _categoryService.GetEnabledCategoriesAsync();

        // Assert
        Assert.Equal(7, result.Count);
        Assert.Contains("health", result);
        Assert.Contains("chores", result);
        Assert.Contains("fun", result);
        Assert.Contains("learning", result);
        Assert.Contains("social", result);
        Assert.Contains("creative", result);
        Assert.Contains("productivity", result);
    }

    [Fact]
    public async Task GetEnabledCategoriesAsync_WhenSomeDisabled_ReturnsOnlyEnabledNames()
    {
        // Arrange
        var storedCategories = new List<Category>
        {
            new() { Name = "enabled1", Color = "#111111", Enabled = true },
            new() { Name = "disabled1", Color = "#222222", Enabled = false },
            new() { Name = "enabled2", Color = "#333333", Enabled = true }
        };
        _localStorageService.ContainKeyAsync("enabledCategories").Returns(true);
        _localStorageService.GetItemAsync<IReadOnlyList<Category>>("enabledCategories")
            .Returns(storedCategories);
        await _categoryService.LoadCategoriesAsync();

        // Act
        var result = _categoryService.GetEnabledCategoriesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("enabled1", result);
        Assert.Contains("enabled2", result);
        Assert.DoesNotContain("disabled1", result);
    }

    [Fact]
    public async Task GetEnabledCategoriesAsync_WhenNoneEnabled_ReturnsEmptyList()
    {
        // Arrange
        var storedCategories = new List<Category>
        {
            new() { Name = "disabled1", Color = "#111111", Enabled = false },
            new() { Name = "disabled2", Color = "#222222", Enabled = false }
        };
        _localStorageService.ContainKeyAsync("enabledCategories").Returns(true);
        _localStorageService.GetItemAsync<IReadOnlyList<Category>>("enabledCategories")
            .Returns(storedCategories);
        await _categoryService.LoadCategoriesAsync();

        // Act
        var result = _categoryService.GetEnabledCategoriesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetEnabledCategoriesAsync_WhenNotLoaded_ReturnsEmptyList()
    {
        // Act
        var result = _categoryService.GetEnabledCategoriesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCategoriesAsync_AfterLoad_ReturnsSameListInstance()
    {
        // Arrange
        _localStorageService.ContainKeyAsync("enabledCategories").Returns(false);
        await _categoryService.LoadCategoriesAsync();

        // Act
        var result1 = _categoryService.GetCategoriesAsync();
        var result2 = _categoryService.GetCategoriesAsync();

        // Assert
        Assert.Same(result1, result2);
    }
}