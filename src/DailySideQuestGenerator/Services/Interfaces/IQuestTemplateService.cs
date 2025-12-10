using DailySideQuestGenerator.Models;

namespace DailySideQuestGenerator.Services.Interfaces;

public interface IQuestTemplateService
{
    Task LoadQuestTemplatesAsync();
    List<QuestTemplate> GetQuestTemplates();
}