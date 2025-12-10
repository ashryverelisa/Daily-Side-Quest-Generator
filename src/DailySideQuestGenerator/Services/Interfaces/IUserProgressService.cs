using DailySideQuestGenerator.Models;

namespace DailySideQuestGenerator.Services.Interfaces;

public interface IUserProgressService
{
    Task LoadUserProgressAsync();
    UserProgress GetUserProgress();
}