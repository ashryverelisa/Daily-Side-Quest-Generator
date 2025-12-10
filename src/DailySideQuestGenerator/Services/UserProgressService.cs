using Blazored.LocalStorage;
using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;

namespace DailySideQuestGenerator.Services;

public class UserProgressService(ILocalStorageService localStorageService) : IUserProgressService
{
    public UserProgress GetUserProgress() => _userProgress;
    private const string UserProgressStorageKey = "userProgress";
    private UserProgress _userProgress = new();
    
    public async Task LoadUserProgressAsync()
    {
        if (await localStorageService.ContainKeyAsync(UserProgressStorageKey))
        {
            _userProgress = (await localStorageService.GetItemAsync<UserProgress>(UserProgressStorageKey))!;
        }
        else
        {     
            _userProgress = new UserProgress();
            await PersistEnabledAsync();
        }
    }
    
    private async Task PersistEnabledAsync()
    {
        await localStorageService.SetItemAsync(UserProgressStorageKey, _userProgress);
    }
}