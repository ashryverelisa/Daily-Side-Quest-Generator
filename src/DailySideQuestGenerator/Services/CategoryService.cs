using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;

namespace DailySideQuestGenerator.Services;

public class CategoryService : ICategoryService
{
    private readonly List<Category> _categories = [];
    private readonly List<string> _enabled = [];

    private bool _initialized = false;

    public Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        if (!_initialized)
            Seed();

        return Task.FromResult<IReadOnlyList<Category>>(_categories);
    }

    public Task<string?> GetCategoryColorAsync(string categoryName)
    {
        if (!_initialized)
            Seed();
        
        return Task.FromResult(_categories.FirstOrDefault(x => x.Name.Equals(categoryName))?.Color);
    }

    public Task<IReadOnlyList<string>> GetEnabledCategoriesAsync()
    {
        if (!_initialized)
            Seed();

        return Task.FromResult<IReadOnlyList<string>>(_enabled);
    }

    public Task SetEnabledCategoriesAsync(List<string> categoryIds)
    {
        _enabled.Clear();
        _enabled.AddRange(categoryIds);
        return Task.CompletedTask;
    }

    private void Seed()
    {
        _categories.Add(new Category { Name = "health", Color = "#28a745" });
        _categories.Add(new Category { Name = "chores", Color = "#ffc107" });
        _categories.Add(new Category { Name = "fun", Color = "#17a2b8" });
        _categories.Add(new Category { Name = "learning", Color = "#6610f2" });
        _categories.Add(new Category { Name = "social", Color = "#e83e8c" });
        _categories.Add(new Category { Name = "creative", Color = "#fd7e14" });
        _categories.Add(new Category { Name = "productivity", Color = "#20c997" });

        // default: all enabled
        _enabled.AddRange(_categories.Select(c => c.Name));

        _initialized = true;
    }
}