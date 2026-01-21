using Blazored.LocalStorage;
using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;

namespace DailySideQuestGenerator.Services;

public class CategoryService(ILocalStorageService localStorageService) : ICategoryService
{
    private const string EnabledCategoriesStorageKey = "enabledCategories";
    private List<Category> _categories = [];

    public List<Category> GetCategoriesAsync() => _categories;

    public async Task LoadCategoriesAsync()
    {
        if (await localStorageService.ContainKeyAsync(EnabledCategoriesStorageKey))
        {
            var categories = await localStorageService.GetItemAsync<IReadOnlyList<Category>>(EnabledCategoriesStorageKey);
            _categories = categories!.ToList();
        }
        else
        {     
            SeedCategories();
            await PersistEnabledAsync();
        }
    }
    
    public string? GetCategoryColor(string categoryName) => _categories.FirstOrDefault(x => x.Name.Equals(categoryName))?.Color;

    public IReadOnlyList<string> GetEnabledCategories() => _categories.Where(x => x.Enabled).Select(x => x.Name).ToList();

    private void SeedCategories()
    {
        if (_categories.Count != 0) return;

        _categories.Add(new Category { Name = "health", Color = "#28a745" });
        _categories.Add(new Category { Name = "chores", Color = "#ffc107" });
        _categories.Add(new Category { Name = "fun", Color = "#17a2b8" });
        _categories.Add(new Category { Name = "learning", Color = "#6610f2" });
        _categories.Add(new Category { Name = "social", Color = "#e83e8c" });
        _categories.Add(new Category { Name = "creative", Color = "#fd7e14" });
        _categories.Add(new Category { Name = "productivity", Color = "#20c997" });
    }

    private async Task PersistEnabledAsync()
    {
        var snapshot = _categories.ToList();
        await localStorageService.SetItemAsync(EnabledCategoriesStorageKey, snapshot);
    }
}