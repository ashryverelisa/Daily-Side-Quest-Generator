using DailySideQuestGenerator.Models;

namespace DailySideQuestGenerator.Services.Interfaces;

public interface IQuestTemplateService
{
    Task LoadQuestTemplatesAsync();
    List<QuestTemplate> GetQuestTemplates();
    QuestTemplate? GetQuestTemplateById(Guid id);
    Task AddQuestTemplateAsync(QuestTemplate template);
    Task UpdateQuestTemplateAsync(QuestTemplate template);
    Task DeleteQuestTemplateAsync(Guid id);
    Task ToggleActiveAsync(Guid id);
}