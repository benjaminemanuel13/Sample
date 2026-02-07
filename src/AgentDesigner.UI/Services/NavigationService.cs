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
}
