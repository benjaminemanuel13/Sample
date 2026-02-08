using AgentDesigner.Domain.Entities;

namespace AgentDesigner.UI.Services;

/// <summary>
/// Simple service to pass data between views without using Shell navigation parameters.
/// </summary>
public class NavigationService
{
    private Workflow? _currentWorkflow;

    public void SetCurrentWorkflow(Workflow workflow)
    {
        _currentWorkflow = workflow;
    }

    public Workflow? GetCurrentWorkflow()
    {
        var workflow = _currentWorkflow;
        _currentWorkflow = null; // Clear after retrieval
        return workflow;
    }

    private DomainModel? _currentModel;

    public void SetCurrentModel(DomainModel model)
    {
        _currentModel = model;
    }

    public DomainModel? GetCurrentModel()
    {
        var model = _currentModel;
        _currentModel = null;
        return model;
    }

    private Project? _currentProject;

    public void SetCurrentProject(Project project)
    {
        _currentProject = project;
    }

    public Project? GetCurrentProject()
    {
        return _currentProject; // Don't clear, as it's needed across multiple operations
    }

    public Task NavigateToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }
}
