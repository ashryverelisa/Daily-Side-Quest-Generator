using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace DailySideQuestGenerator.Components.Shared;

public partial class QuestCard
{
    [Parameter] public DailyQuest Quest { get; set; } = null!;
    [Parameter] public EventCallback<QuestToggleResult> OnToggled { get; set; }
    [Inject] private IQuestService QuestService { get; set; } = null!;
    [Inject] private ICategoryService CategoryService { get; set; } = null!;

    private string ButtonText => Quest.IsCompleted ? "Completed ✓" : "Complete";
    private string CompleteButtonClass => Quest.IsCompleted ? "complete-btn completed" : "complete-btn";
    private string CompletedClass => Quest.IsCompleted ? "completed" : "";
    private string _categoryColor = "#555";
    private string _categoryName = "Unknown";
    private bool _justCompleted;
    
    private string JustCompletedClass => _justCompleted ? "just-completed" : "";
    
    // Category icon mapping for RPG theme
    private string CategoryIcon => _categoryName.ToLowerInvariant() switch
    {
        "health" or "fitness" or "exercise" => "💪",
        "learning" or "education" or "study" => "📚",
        "creativity" or "creative" or "art" => "🎨",
        "social" or "relationships" or "friends" => "🤝",
        "productivity" or "work" or "career" => "⚡",
        "wellness" or "self-care" or "mindfulness" => "🧘",
        "home" or "household" or "chores" => "🏠",
        "finance" or "money" or "budget" => "💰",
        "adventure" or "outdoor" or "nature" => "🌲",
        "gaming" or "entertainment" or "fun" => "🎮",
        "food" or "cooking" or "nutrition" => "🍳",
        "music" or "audio" => "🎵",
        "writing" or "journal" => "✍️",
        "reading" or "books" => "📖",
        "tech" or "technology" or "coding" => "💻",
        "meditation" or "spiritual" => "🕯️",
        "cleaning" or "organization" => "✨",
        "sleep" or "rest" => "😴",
        "hydration" or "water" => "💧",
        "pet" or "pets" or "animals" => "🐾",
        _ => "⚔️" // Default RPG sword icon
    };

    protected override void OnInitialized()
    {
        var category = CategoryService.GetCategoryColorAsync(Quest.Category);
        
        if (category != null)
        {
            _categoryColor = category;
            _categoryName = Quest.Category;
        }
    }
    
    private async Task ToggleComplete()
    {
        var wasCompleted = Quest.IsCompleted;
        var result = await QuestService.ToggleCompleteAsync(Quest.Id);
        
        // Trigger animation if quest was just completed
        if (!wasCompleted && result.WasCompleted)
        {
            _justCompleted = true;
            StateHasChanged();
            
            // Reset animation class after animation completes
            await Task.Delay(600);
            _justCompleted = false;
        }
        
        if (OnToggled.HasDelegate) await OnToggled.InvokeAsync(result);
    }
}