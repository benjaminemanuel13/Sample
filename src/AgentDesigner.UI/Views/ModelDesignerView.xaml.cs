using AgentDesigner.UI.ViewModels;
using AgentDesigner.Domain.Entities;
using Microsoft.Maui.Graphics;

namespace AgentDesigner.UI.Views;

public partial class ModelDesignerView : ContentPage
{
    private ModelEntity? _draggedEntity;
    private PointF _dragStartPoint;
    private bool _isPanning;
    private PointF _panStartPoint;
    private double _initialOffsetX;
    private double _initialOffsetY;

    public ModelDesignerView(ModelDesignerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        viewModel.RequestInvalidate += (s, e) => ModelCanvas?.Invalidate();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ModelDesignerViewModel vm)
        {
            vm.OnNavigatedTo();
        }
    }

    private void OnCanvasStartInteraction(object sender, TouchEventArgs e)
    {
        var vm = BindingContext as ModelDesignerViewModel;
        if (vm == null) return;

        var screenPoint = e.Touches.FirstOrDefault();
        var worldPoint = ToWorldSpace(vm, screenPoint);

        // Check for Entity Selection
        // Simple hit test against entity rectangles
        var selectedEntity = vm.Entities.FirstOrDefault(ent =>
            worldPoint.X >= ent.X && worldPoint.X <= ent.X + 200 &&
            worldPoint.Y >= ent.Y && worldPoint.Y <= ent.Y + (40 + ent.Properties.Count * 25 + 10));

        if (selectedEntity != null)
        {
            vm.SelectedEntity = selectedEntity;
            _draggedEntity = selectedEntity;
            _dragStartPoint = new PointF((float)(worldPoint.X - selectedEntity.X), (float)(worldPoint.Y - selectedEntity.Y));
        }
        else
        {
            vm.SelectedEntity = null;
            _isPanning = true;
            _panStartPoint = screenPoint;
            _initialOffsetX = vm.CanvasOffsetX;
            _initialOffsetY = vm.CanvasOffsetY;
        }

        ModelCanvas.Invalidate();
    }

    private void OnCanvasDragInteraction(object sender, TouchEventArgs e)
    {
        var vm = BindingContext as ModelDesignerViewModel;
        if (vm == null) return;

        var screenPoint = e.Touches.FirstOrDefault();
        var worldPoint = ToWorldSpace(vm, screenPoint);

        if (_draggedEntity != null)
        {
            _draggedEntity.X = worldPoint.X - _dragStartPoint.X;
            _draggedEntity.Y = worldPoint.Y - _dragStartPoint.Y;
            ModelCanvas.Invalidate();
        }
        else if (_isPanning)
        {
            var deltaX = screenPoint.X - _panStartPoint.X;
            var deltaY = screenPoint.Y - _panStartPoint.Y;

            vm.CanvasOffsetX = _initialOffsetX + deltaX;
            vm.CanvasOffsetY = _initialOffsetY + deltaY;
            ModelCanvas.Invalidate();
        }
    }

    private void OnCanvasEndInteraction(object sender, TouchEventArgs e)
    {
        _draggedEntity = null;
        _isPanning = false;
        ModelCanvas?.Invalidate();
    }

    private PointF ToWorldSpace(ModelDesignerViewModel vm, PointF screenPoint)
    {
        return new PointF(
            (float)((screenPoint.X - vm.CanvasOffsetX) / vm.CanvasZoom),
            (float)((screenPoint.Y - vm.CanvasOffsetY) / vm.CanvasZoom));
    }
}
