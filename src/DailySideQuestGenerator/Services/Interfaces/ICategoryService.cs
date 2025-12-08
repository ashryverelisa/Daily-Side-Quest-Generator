using DailySideQuestGenerator.Models;

namespace DailySideQuestGenerator.Services.Interfaces;

public interface ICategoryService
{
    List<Category> GetCategoriesAsync();
    Task LoadCategoriesAsync();
    string? GetCategoryColorAsync(string categoryName);
    IReadOnlyList<string> GetEnabledCategoriesAsync();
}