using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace DailySideQuestGenerator.Components.Pages;

public partial class Home
{
    [Inject] private IQuestService QuestService { get; set; } = null!;
    [Inject] private ICategoryService CategoryService { get; set; } = null!;
    [Inject] private IUserProgressService UserProgressService { get; set; } = null!;
    
    private List<DailyQuest>? _quests;
    private IReadOnlyList<Category>? _categories;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await QuestService.InitializeIfNeededAsync();
            await CategoryService.LoadCategoriesAsync();
            await LoadDataAsync();    
        }
    }

    private async Task LoadDataAsync()
    {
        _quests = (await QuestService.GetTodaysQuestsAsync()).ToList();
        _categories = CategoryService.GetCategoriesAsync();
        StateHasChanged();
    }

    private async Task OnToggled()
    {
        await LoadDataAsync();
    }
}