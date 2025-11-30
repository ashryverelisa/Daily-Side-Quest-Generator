using DailySideQuestGenerator.Models;

namespace DailySideQuestGenerator.Services.Interfaces;

public interface ICategoryService
{
    /// <summary>
    /// Asynchronously gets the list of all available categories.
    /// </summary>
    /// <returns>A read-only list of <see cref="Category"/> objects representing all categories.</returns>
    Task<IReadOnlyList<Category>> GetCategoriesAsync();
    
    /// <summary>
    /// Asynchronously resolves the hexadecimal color associated with a category name.
    /// </summary>
    /// <param name="categoryName">The display name of the category to inspect.</param>
    /// <returns>The color string (e.g., hex) if the category exists; otherwise <c>null</c>.</returns>
    Task<string?> GetCategoryColorAsync(string categoryName);

    /// <summary>
    /// Asynchronously sets which categories are enabled by their identifiers.
    /// </summary>
    /// <param name="categoryIds">A list of category id strings to mark as enabled. Passing an empty list will disable all categories.</param>
    Task SetEnabledCategoriesAsync(List<string> categoryIds);

    /// <summary>
    /// Asynchronously gets the identifiers of the currently enabled categories.
    /// </summary>
    /// <returns>A read-only list of category id strings that are currently enabled.</returns>
    Task<IReadOnlyList<string>> GetEnabledCategoriesAsync();
}