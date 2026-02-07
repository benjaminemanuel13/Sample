namespace AgentDesigner.UI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        Routing.RegisterRoute(nameof(Views.WorkflowDesignerView), typeof(Views.WorkflowDesignerView));
        Routing.RegisterRoute(nameof(Views.ModelDesignerView), typeof(Views.ModelDesignerView));
    }
}
