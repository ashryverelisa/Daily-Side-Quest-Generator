using DailySideQuestGenerator.Models;

namespace DailySideQuestGenerator.Services.Interfaces;

public interface ICategoryService
{
    List<Category> GetCategoriesAsync();
    Task LoadCategoriesAsync();
    string? GetCategoryColor(string categoryName);
    IReadOnlyList<string> GetEnabledCategories();
}