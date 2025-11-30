using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace DailySideQuestGenerator.Components.Pages;

public partial class Home
{
    [Inject] private IQuestService QuestService { get; set; } = null!;
    [Inject] private ICategoryService CategoryService { get; set; } = null!;
    
    private List<DailyQuest>? _quests;
    private UserProgress? _progress;
    private IReadOnlyList<Category>? _categories;

    protected override async Task OnInitializedAsync()
    {
        await QuestService.InitializeIfNeededAsync();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _quests = (await QuestService.GetTodaysQuestsAsync()).ToList();
        _progress = await QuestService.GetProgressAsync();
        _categories = await CategoryService.GetCategoriesAsync();
        StateHasChanged();
    }

    private async Task OnToggled()
    {
        _progress = await QuestService.GetProgressAsync();
        await LoadDataAsync();
    }
    private string GetCategoryColor(string cat)
    {
        return _categories?
                   .FirstOrDefault(c => c.Name == cat)?.Color 
               ?? "#777";
    }
}