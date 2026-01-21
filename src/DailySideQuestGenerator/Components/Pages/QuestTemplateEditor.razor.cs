using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace DailySideQuestGenerator.Components.Pages;

public partial class QuestTemplateEditor : ComponentBase
{
    [Inject] public IQuestTemplateService QuestTemplateService { get; set; } = default!;
    [Inject] public ICategoryService CategoryService { get; set; } = default!;

    private List<QuestTemplate>? _templates;
    private List<Category> _categories = [];
    private QuestTemplateFormModel _formModel = new();
    private bool _isEditing;
    private Guid? _editingId;
    
    // Delete confirmation
    private bool _showDeleteConfirm;
    private QuestTemplate? _templateToDelete;
    
    // Filtering
    private string _searchTerm = string.Empty;
    private string _filterCategory = string.Empty;
    private string _filterStatus = string.Empty;

    private IEnumerable<QuestTemplate> FilteredTemplates
    {
        get
        {
            if (_templates is null) return [];
            
            var filtered = _templates.AsEnumerable();
            
            if (!string.IsNullOrWhiteSpace(_searchTerm))
            {
                filtered = filtered.Where(t => 
                    t.Title.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            
            if (!string.IsNullOrWhiteSpace(_filterCategory))
            {
                filtered = filtered.Where(t => t.Category == _filterCategory);
            }
            
            if (!string.IsNullOrWhiteSpace(_filterStatus))
            {
                filtered = _filterStatus switch
                {
                    "active" => filtered.Where(t => t.IsActive),
                    "inactive" => filtered.Where(t => !t.IsActive),
                    _ => filtered
                };
            }
            
            return filtered.OrderBy(t => t.Category).ThenBy(t => t.Title);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadDataAsync();
            StateHasChanged();
        }
    }

    private async Task LoadDataAsync()
    {
        await CategoryService.LoadCategoriesAsync();
        _categories = CategoryService.GetCategoriesAsync();
        
        await QuestTemplateService.LoadQuestTemplatesAsync();
        _templates = QuestTemplateService.GetQuestTemplates();
        
        // Set default category if available
        if (_categories.Count != 0 && string.IsNullOrEmpty(_formModel.Category))
        {
            _formModel.Category = _categories.First().Name;
        }
    }

    private async Task HandleValidSubmit()
    {
        if (_isEditing && _editingId.HasValue)
        {
            var template = new QuestTemplate
            {
                Id = _editingId.Value,
                Title = _formModel.Title,
                Description = _formModel.Description,
                BaseXp = _formModel.BaseXp,
                Category = _formModel.Category,
                RarityWeight = _formModel.RarityWeight,
                IsActive = _formModel.IsActive
            };
            await QuestTemplateService.UpdateQuestTemplateAsync(template);
        }
        else
        {
            var template = new QuestTemplate
            {
                Title = _formModel.Title,
                Description = _formModel.Description,
                BaseXp = _formModel.BaseXp,
                Category = _formModel.Category,
                RarityWeight = _formModel.RarityWeight,
                IsActive = _formModel.IsActive
            };
            await QuestTemplateService.AddQuestTemplateAsync(template);
        }
        
        ResetForm();
        _templates = QuestTemplateService.GetQuestTemplates();
    }

    private void StartEdit(QuestTemplate template)
    {
        _isEditing = true;
        _editingId = template.Id;
        _formModel = new QuestTemplateFormModel
        {
            Title = template.Title,
            Description = template.Description,
            BaseXp = template.BaseXp,
            Category = template.Category,
            RarityWeight = template.RarityWeight,
            IsActive = template.IsActive
        };
    }

    private void CancelEdit()
    {
        ResetForm();
    }

    private void ResetForm()
    {
        _isEditing = false;
        _editingId = null;
        _formModel = new QuestTemplateFormModel
        {
            Category = _categories.FirstOrDefault()?.Name ?? "general"
        };
    }

    private async Task ToggleActive(Guid id)
    {
        await QuestTemplateService.ToggleActiveAsync(id);
        _templates = QuestTemplateService.GetQuestTemplates();
    }

    private void ConfirmDelete(QuestTemplate template)
    {
        _templateToDelete = template;
        _showDeleteConfirm = true;
    }

    private void CancelDelete()
    {
        _templateToDelete = null;
        _showDeleteConfirm = false;
    }

    private async Task ExecuteDelete()
    {
        if (_templateToDelete is not null)
        {
            await QuestTemplateService.DeleteQuestTemplateAsync(_templateToDelete.Id);
            _templates = QuestTemplateService.GetQuestTemplates();
            
            // If we were editing the deleted template, reset the form
            if (_editingId == _templateToDelete.Id)
            {
                ResetForm();
            }
        }
        CancelDelete();
    }

    private string GetCategoryColor(string categoryName)
    {
        return CategoryService.GetCategoryColor(categoryName) ?? "#888888";
    }
}