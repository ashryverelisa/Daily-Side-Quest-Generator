using Blazored.LocalStorage;
using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;

namespace DailySideQuestGenerator.Services;

public class UserProgressService(ILocalStorageService localStorageService) : IUserProgressService
{
    private const string StorageKey = "userProgress";
    private const int XpPerLevel = 10;
    
    public UserProgress Progress { get; private set; } = new();

    public async Task LoadAsync()
    {
        var loaded = await localStorageService.GetItemAsync<UserProgress>(StorageKey);
        if (loaded is not null)
        {
            Progress = loaded;
        }
        else
        {
            Progress = new UserProgress();
            await SaveAsync();
        }
    }

    public Task SaveAsync() => localStorageService.SetItemAsync(StorageKey, Progress).AsTask();

    public async Task AddXpAsync(int xp)
    {
        Progress.TotalXp += xp;
        Progress.Level = CalculateLevel(Progress.TotalXp);
        await SaveAsync();
    }

    public async Task RemoveXpAsync(int xp)
    {
        Progress.TotalXp = Math.Max(0, Progress.TotalXp - xp);
        Progress.Level = CalculateLevel(Progress.TotalXp);
        await SaveAsync();
    }

    public async Task UpdateStreakAsync()
    {
        var today = DateTime.UtcNow.Date;
        var lastCompleted = Progress.LastQuestCompleted.Date;

        if (lastCompleted == today.AddDays(-1))
        {
            Progress.DailyStreak++;
        }
        else if (lastCompleted != today)
        {
            Progress.DailyStreak = 1;
        }
        
        Progress.LastQuestCompleted = DateTime.UtcNow;
        await SaveAsync();
    }

    public async Task DecrementStreakAsync()
    {
        Progress.DailyStreak = Math.Max(0, Progress.DailyStreak - 1);
        await SaveAsync();
    }

    private static int CalculateLevel(int totalXp) =>
        Math.Max(1, (int)Math.Floor(Math.Sqrt(totalXp / (double)XpPerLevel)) + 1);
}