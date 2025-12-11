using DailySideQuestGenerator.Models;

namespace DailySideQuestGenerator.Services.Interfaces;

public interface IDailyQuestService
{
    List<DailyQuest> DailyQuests { get; set; }
    Task LoadAsync();
    Task SaveAsync();
}