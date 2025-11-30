using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace DailySideQuestGenerator.Components.Shared;

public partial class QuestCard
{
    [Parameter] public DailyQuest Quest { get; set; } = null!;
    [Parameter] public EventCallback OnToggled { get; set; }
    [Inject] private IQuestService QuestService { get; set; } = null!;
    [Inject] private ICategoryService CategoryService { get; set; } = null!;

    private string ButtonText => Quest.IsCompleted ? "Completed" : "Complete";
    private string CompleteButtonClass => Quest.IsCompleted ? "complete-btn completed" : "complete-btn";
    private string _categoryColor = "#555";
    private string _categoryName = "Unknown";
    
    protected override async Task OnInitializedAsync()
    {
        var category = await CategoryService.GetCategoryColorAsync(Quest.Category);
        
        if (category != null)
        {
            _categoryColor = category;
            _categoryName = Quest.Category;
        }
    }

    private async Task ToggleComplete()
    {
        await QuestService.ToggleCompleteAsync(Quest.Id);
        if (OnToggled.HasDelegate) await OnToggled.InvokeAsync(null);
    }
}