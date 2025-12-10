using Blazored.LocalStorage;
using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;

namespace DailySideQuestGenerator.Services;

public class QuestTemplateService(ILocalStorageService localStorageService) : IQuestTemplateService
{
    private const string QuestTemplateStorageKey = "questTemplates";
    private List<QuestTemplate> _questTemplates = [];
    public List<QuestTemplate> GetQuestTemplates() => _questTemplates;
    
    public async Task LoadQuestTemplatesAsync()
    {
        if (await localStorageService.ContainKeyAsync(QuestTemplateStorageKey))
        {
            _questTemplates = (await localStorageService.GetItemAsync<List<QuestTemplate>>(QuestTemplateStorageKey))!;
        }
        else
        {
            SeedTemplates();
            await PersistEnabledAsync();
        }
    }

    private void SeedTemplates()
    {
        if (_questTemplates.Count != 0) return;

        _questTemplates =
        [
            new QuestTemplate { Title = "Drink 1L of water", BaseXp = 5, Category = "health", RarityWeight = 1 },
            new QuestTemplate { Title = "Clean your desk", BaseXp = 8, Category = "chores", RarityWeight = 2 },
            new QuestTemplate { Title = "Watch 1 new anime episode", BaseXp = 3, Category = "fun", RarityWeight = 3 },
            new QuestTemplate { Title = "Stretch for 5 minutes", BaseXp = 4, Category = "health", RarityWeight = 1 },
            new QuestTemplate { Title = "Read 10 pages of a book", BaseXp = 6, Category = "learning", RarityWeight = 2 },
            new QuestTemplate { Title = "Send a message to a friend", BaseXp = 6, Category = "social", RarityWeight = 3 },
            new QuestTemplate { Title = "Tidy one shelf", BaseXp = 7, Category = "chores", RarityWeight = 3 },
            new QuestTemplate { Title = "Try a 5-minute breathing exercise", BaseXp = 4, Category = "health", RarityWeight = 2 },
            new QuestTemplate { Title = "Sketch something for 10 minutes", BaseXp = 5, Category = "creative", RarityWeight = 4 },
            new QuestTemplate { Title = "Plan tomorrow for 5 minutes", BaseXp = 4, Category = "productivity", RarityWeight = 2 }
        ];
    }

    private async Task PersistEnabledAsync()
    {
        var snapshot = _questTemplates.ToList();
        await localStorageService.SetItemAsync(QuestTemplateStorageKey, snapshot);
    }
}