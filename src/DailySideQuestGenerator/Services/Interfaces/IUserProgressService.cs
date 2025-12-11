using DailySideQuestGenerator.Models;

namespace DailySideQuestGenerator.Services.Interfaces;

public interface IUserProgressService
{
    UserProgress Progress { get; }
    
    Task LoadAsync();
    Task SaveAsync();
    
    Task AddXpAsync(int xp);
    Task RemoveXpAsync(int xp);
    Task UpdateStreakAsync();
    Task DecrementStreakAsync();
}