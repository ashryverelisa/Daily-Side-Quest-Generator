using DailySideQuestGenerator.Models;

namespace DailySideQuestGenerator.Components.Pages;

public partial class Home
{
    private List<DailyQuest>? _quests;
    private UserProgress? _progress;

    protected override async Task OnInitializedAsync()
    {
        await QuestService.InitializeIfNeededAsync();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var readOnlyQuests = await QuestService.GetTodaysQuestsAsync();
        _quests = readOnlyQuests.ToList();
        _progress = await QuestService.GetProgressAsync();
        StateHasChanged();
    }

    private async Task OnToggled()
    {
        _progress = await QuestService.GetProgressAsync();
        await LoadDataAsync();
    }
}