using Blazored.LocalStorage;
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
        _progress = await QuestService.GetProgressAsync();
        _categories = CategoryService.GetCategoriesAsync();
        StateHasChanged();
    }

    private async Task OnToggled()
    {
        _progress = await QuestService.GetProgressAsync();
        await LoadDataAsync();
    }
}