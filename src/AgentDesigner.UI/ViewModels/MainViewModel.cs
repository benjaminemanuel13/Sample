using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentDesigner.Domain.Entities;
using AgentDesigner.Infrastructure.Persistence;
using AgentDesigner.UI.Services;

namespace AgentDesigner.UI.ViewModels;

/// <summary>
/// Main view model for the application shell.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly JsonProjectRepository _projectRepository;
    private readonly NavigationService _navigationService;
    private readonly WorkflowDesignerViewModel _workflowDesignerViewModel;
    private readonly SqliteMetadataRepository _metadataRepository;

    [ObservableProperty]
    private Project? _currentProject;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMainViewVisible))]
    private bool _showWorkflowDesigner = false;

    public bool IsMainViewVisible => !ShowWorkflowDesigner;

    public WorkflowDesignerViewModel WorkflowDesigner => _workflowDesignerViewModel;

    public MainViewModel(
        JsonProjectRepository projectRepository,
        NavigationService navigationService,
        WorkflowDesignerViewModel workflowDesignerViewModel,
        SqliteMetadataRepository metadataRepository)
    {
        _projectRepository = projectRepository;
        _navigationService = navigationService;
        _workflowDesignerViewModel = workflowDesignerViewModel;
        _metadataRepository = metadataRepository;
    }

    [RelayCommand]
    private async Task NewProject()
    {
        CurrentProject = new Project
        {
            Name = "New Project",
            Description = "A new AI agent workflow project"
        };

        await _projectRepository.CreateAsync(CurrentProject);
        _navigationService.SetCurrentProject(CurrentProject);
        StatusMessage = $"Created new project: {CurrentProject.Name}";
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Open Project",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".json" } }
                })
            });

            if (result != null)
            {
                var project = await _projectRepository.LoadFromFileAsync(result.FullPath);
                CurrentProject = project;
                _navigationService.SetCurrentProject(project);
                StatusMessage = $"Loaded project: {project.Name}";

                // Load first workflow if exists
                if (project.Workflows.Count > 0)
                {
                    var firstWorkflow = project.Workflows[0];
                    _navigationService.SetCurrentWorkflow(firstWorkflow);
                    _workflowDesignerViewModel.LoadWorkflow(firstWorkflow);
                    ShowWorkflowDesigner = true;
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening project: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveProject()
    {
        if (CurrentProject == null)
        {
            StatusMessage = "No project to save";
            return;
        }

        try
        {
            if (string.IsNullOrEmpty(CurrentProject.FilePath))
            {
                // Default to MyDocuments/AgentDesigner/Projects if no path set
                var docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var projectDir = Path.Combine(docsPath, "AgentDesigner", "Projects");
                Directory.CreateDirectory(projectDir);

                var fileName = $"{CurrentProject.Name.Replace(" ", "_")}.json";
                CurrentProject.FilePath = Path.Combine(projectDir, fileName);
            }

            if (CurrentProject.Workflows.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Saving Project. First Workflow Nodes: {CurrentProject.Workflows[0].Nodes.Count}");
            }

            await _projectRepository.UpdateAsync(CurrentProject);
            StatusMessage = $"Saved project to: {CurrentProject.FilePath}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving project: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task NewWorkflow()
    {
        if (CurrentProject == null)
        {
            StatusMessage = "Please create or open a project first";
            return;
        }

        var workflow = new Workflow
        {
            Name = "New Workflow",
            Description = "A new AI agent workflow"
        };

        CurrentProject.Workflows.Add(workflow);
        await _projectRepository.UpdateAsync(CurrentProject);

        // Use navigation service to pass workflow
        _navigationService.SetCurrentWorkflow(workflow);

        // Load workflow in designer
        _workflowDesignerViewModel.OnNavigatedTo();

        // Switch to workflow designer view
        ShowWorkflowDesigner = true;

        StatusMessage = $"Created workflow: {workflow.Name}";
    }

    [RelayCommand]
    private void CloseWorkflowDesigner()
    {
        ShowWorkflowDesigner = false;
    }

    [RelayCommand]
    private async Task NewDataModel()
    {
        if (CurrentProject == null)
        {
            StatusMessage = "Please create or open a project first";
            return;
        }

        var model = new DomainModel
        {
            Name = "New Model",
            Description = "A new data domain model"
        };
        CurrentProject.Models.Add(model);
        await _projectRepository.UpdateAsync(CurrentProject);

        // Pass model to designer
        _navigationService.SetCurrentModel(model);
        await _navigationService.NavigateToAsync(nameof(Views.ModelDesignerView));

        StatusMessage = $"Created data model: {model.Name}";
    }

    [RelayCommand]
    private async Task OpenDataModel(DomainModel model)
    {
        if (model == null) return;

        // Pass model to designer
        _navigationService.SetCurrentModel(model);
        await _navigationService.NavigateToAsync(nameof(Views.ModelDesignerView));

        StatusMessage = $"Opened data model: {model.Name}";
    }

    [RelayCommand]
    private void OpenWorkflow(Workflow workflow)
    {
        if (workflow == null) return;

        // Use navigation service to pass workflow
        _navigationService.SetCurrentWorkflow(workflow);

        // Load workflow in designer
        _workflowDesignerViewModel.OnNavigatedTo();

        // Switch to workflow designer view
        ShowWorkflowDesigner = true;

        StatusMessage = $"Opened workflow: {workflow.Name}";
    }
}
