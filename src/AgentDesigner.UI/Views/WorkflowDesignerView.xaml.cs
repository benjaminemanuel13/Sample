using AgentDesigner.UI.ViewModels;
using Microsoft.Maui.Graphics;

namespace AgentDesigner.UI.Views;

public partial class WorkflowDesignerView : ContentPage
{
    private readonly WorkflowDesignerViewModel _viewModel;

    public WorkflowDesignerView(WorkflowDesignerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnNavigatedTo();
    }
}

/// <summary>
/// Drawable for grid pattern background on canvas.
/// </summary>
public class GridPatternDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = Colors.LightGray;
        canvas.StrokeSize = 1;

        // Draw vertical lines
        for (float x = 0; x < dirtyRect.Width; x += 50)
        {
            canvas.DrawLine(x, 0, x, dirtyRect.Height);
        }

        // Draw horizontal lines
        for (float y = 0; y < dirtyRect.Height; y += 50)
        {
            canvas.DrawLine(0, y, dirtyRect.Width, y);
        }
    }
}
