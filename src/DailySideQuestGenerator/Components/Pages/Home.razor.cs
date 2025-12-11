using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services;
using DailySideQuestGenerator.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace DailySideQuestGenerator.Components.Pages;

public partial class Home : IDisposable
{
    [Inject] private IQuestService QuestService { get; set; } = null!;
    [Inject] private ICategoryService CategoryService { get; set; } = null!;
    [Inject] private IXpService XpService { get; set; } = null!;
    [Inject] private ISoundService SoundService { get; set; } = null!;
    
    private List<DailyQuest>? _quests;
    private LevelInfo? _levelInfo;
    private XpEvent? _lastXpEvent;
    private bool _showXpNotification;
    private bool _animateXpBar;
    private bool _showLevelUpAnimation;
    private System.Timers.Timer? _notificationTimer;

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
        _levelInfo = XpService.GetLevelInfo();
        StateHasChanged();
    }

    private async Task OnToggled(QuestToggleResult result)
    {
        if (result is { WasCompleted: true, XpEvent: not null })
        {
            _lastXpEvent = result.XpEvent;
            _showXpNotification = true;
            _animateXpBar = true;
            _showLevelUpAnimation = result.XpEvent.LeveledUp;
            
            if (result.XpEvent.LeveledUp)
            {
                await SoundService.PlayLevelUpAsync();
            }
            else
            {
                await SoundService.PlayXpGainAsync();
            }
            
            // Auto-dismiss notification after 4 seconds (longer for level up)
            var delay = result.XpEvent.LeveledUp ? 5000 : 3500;
            StartNotificationTimer(delay);
        }
        
        _levelInfo = result.LevelInfo;
        _quests = (await QuestService.GetTodaysQuestsAsync()).ToList();
        StateHasChanged();
        
        // Reset animation flags after a short delay
        await Task.Delay(600);
        _animateXpBar = false;
        
        if (!_showXpNotification)
        {
            _showLevelUpAnimation = false;
        }
        
        StateHasChanged();
    }

    private void StartNotificationTimer(int delay)
    {
        _notificationTimer?.Stop();
        _notificationTimer?.Dispose();
        
        _notificationTimer = new System.Timers.Timer(delay);
        _notificationTimer.Elapsed += async (_, _) =>
        {
            _notificationTimer?.Stop();
            await InvokeAsync(() =>
            {
                DismissNotification();
                StateHasChanged();
            });
        };
        _notificationTimer.AutoReset = false;
        _notificationTimer.Start();
    }

    private void DismissNotification()
    {
        _showXpNotification = false;
        _showLevelUpAnimation = false;
        _notificationTimer?.Stop();
        _notificationTimer?.Dispose();
        _notificationTimer = null;
        StateHasChanged();
    }

    public void Dispose()
    {
        _notificationTimer?.Stop();
        _notificationTimer?.Dispose();
    }
}