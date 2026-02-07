#nullable disable
using AgentDesigner.UI.ViewModels;
using System.Linq;
using AgentDesigner.Domain.Entities;
using AgentDesigner.Domain.Enums;
using Microsoft.Maui.Graphics;

namespace AgentDesigner.UI.Views;

public partial class MainView : ContentPage
{
    private WorkflowNode? _draggedNode;
    private PointF _dragStartPoint;

    private WorkflowNode? _connectionSourceNode;
    private string? _connectionSourcePortType; // "true", "false", or null if standard
    private PointF _connectionEndPoint;
    private bool _isCreatingConnection;
    private bool _isPanning;
    private PointF _panStartPoint; // Screen space
    private double _initialOffsetX;
    private double _initialOffsetY;

    public MainView(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        WorkflowCanvas.StartInteraction += OnCanvasStartInteraction;
        WorkflowCanvas.DragInteraction += OnCanvasDragInteraction;
        WorkflowCanvas.EndInteraction += OnCanvasEndInteraction;
    }

    public void OnCanvasStartInteraction(object sender, TouchEventArgs e)
    {
        var mainViewModel = BindingContext as MainViewModel;
        var vm = mainViewModel?.WorkflowDesigner;
        if (vm == null || vm.CurrentWorkflow == null) return;

        var screenPoint = e.Touches.FirstOrDefault();
        var worldPoint = ToWorldSpace(vm, screenPoint);

        // Check for ports first (priority over dragging)
        var (portNode, isOutput, portType) = FindPortAtPoint(vm, worldPoint);

        if (portNode != null)
        {
            if (isOutput)
            {
                // Start connection
                _isCreatingConnection = true;
                _connectionSourceNode = portNode;
                _connectionSourcePortType = portType;
                _connectionEndPoint = worldPoint;

                // Update temp connection in ViewModel (ViewModel uses World Space)
                vm.TempConnectionStart = GetPortPosition(portNode, isOutput, portType);
                vm.TempConnectionEnd = worldPoint;
            }
            return;
        }

        // Check for node selection/dragging
        var selectedNode = vm.Nodes
            .FirstOrDefault(n => worldPoint.X >= n.X && worldPoint.X <= n.X + 150 &&
                                 worldPoint.Y >= n.Y && worldPoint.Y <= n.Y + (n.NodeType == NodeType.DecisionNode ? 120 : 100));

        if (selectedNode != null)
        {
            vm.SelectedNode = selectedNode;
            _draggedNode = selectedNode;
            // Drag offset must remain in world space
            _dragStartPoint = new PointF((float)(worldPoint.X - selectedNode.X), (float)(worldPoint.Y - selectedNode.Y));
        }
        else
        {
            // Start Panning (using Screen Space)
            vm.SelectedNode = null;
            _isPanning = true;
            _panStartPoint = screenPoint;
            _initialOffsetX = vm.CanvasOffsetX;
            _initialOffsetY = vm.CanvasOffsetY;
        }

        WorkflowCanvas.Invalidate();
    }

    public void OnCanvasDragInteraction(object sender, TouchEventArgs e)
    {
        var mainViewModel = BindingContext as MainViewModel;
        var vm = mainViewModel?.WorkflowDesigner;
        if (vm == null) return;

        var screenPoint = e.Touches.FirstOrDefault();
        var worldPoint = ToWorldSpace(vm, screenPoint);

        if (_isCreatingConnection)
        {
            _connectionEndPoint = worldPoint;
            vm.TempConnectionEnd = worldPoint;
            WorkflowCanvas.Invalidate();
        }
        else if (_draggedNode != null)
        {
            _draggedNode.X = worldPoint.X - _dragStartPoint.X;
            _draggedNode.Y = worldPoint.Y - _dragStartPoint.Y;
            WorkflowCanvas.Invalidate();
        }
        else if (_isPanning)
        {
            // Panning is in Screen Space logic (offsetting the canvas)
            var deltaX = screenPoint.X - _panStartPoint.X;
            var deltaY = screenPoint.Y - _panStartPoint.Y;

            vm.CanvasOffsetX = _initialOffsetX + deltaX;
            vm.CanvasOffsetY = _initialOffsetY + deltaY;
            WorkflowCanvas.Invalidate();
        }
    }

    public void OnCanvasEndInteraction(object sender, TouchEventArgs e)
    {
        var mainViewModel = BindingContext as MainViewModel;
        var vm = mainViewModel?.WorkflowDesigner;
        if (vm == null) return;

        var screenPoint = e.Touches.FirstOrDefault();
        var worldPoint = ToWorldSpace(vm, screenPoint);

        if (_isCreatingConnection && _connectionSourceNode != null)
        {
            // Check if dropped on an input port
            var (targetNode, isOutput, _) = FindPortAtPoint(vm, worldPoint);

            if (targetNode != null && !isOutput && targetNode != _connectionSourceNode)
            {
                // Create Edge
                var edge = new WorkflowEdge
                {
                    SourceNodeId = _connectionSourceNode.Id,
                    TargetNodeId = targetNode.Id,
                    Label = _connectionSourcePortType // "true", "false", or null
                };

                // Add to ViewModel and Domain
                vm.Edges.Add(edge);
                vm.CurrentWorkflow?.Edges.Add(edge);
            }
        }

        // Reset state
        _isCreatingConnection = false;
        _isPanning = false;
        _connectionSourceNode = null;
        _connectionSourcePortType = null;
        _draggedNode = null;

        vm.TempConnectionStart = PointF.Zero;
        vm.TempConnectionEnd = PointF.Zero;

        WorkflowCanvas.Invalidate();
    }

    private PointF ToWorldSpace(WorkflowDesignerViewModel vm, PointF screenPoint)
    {
        return new PointF(
            (float)((screenPoint.X - vm.CanvasOffsetX) / vm.CanvasZoom),
            (float)((screenPoint.Y - vm.CanvasOffsetY) / vm.CanvasZoom));
    }

    private (WorkflowNode? node, bool isOutput, string? portType) FindPortAtPoint(WorkflowDesignerViewModel vm, PointF point)
    {
        foreach (var node in vm.Nodes)
        {
            // Standard Node (150x100)
            // Input: Top-Center (75, 0)
            // Output: Bottom-Center (75, 100)

            // Decision Node (Diamond)
            // Input: Top-Center (75, 0)
            // True: Right-Center (150, 60) roughly
            // False: Bottom-Center (75, 120)

            float tolerance = 20f; // Hit target size

            if (node.NodeType == NodeType.DecisionNode)
            {
                // Input
                var inputPos = new PointF((float)node.X + 75, (float)node.Y);
                if (Distance(point, inputPos) < tolerance) return (node, false, null);

                // True (Right)
                var truePos = new PointF((float)node.X + 150, (float)node.Y + 60);
                if (Distance(point, truePos) < tolerance) return (node, true, "true");

                // False (Bottom)
                var falsePos = new PointF((float)node.X + 75, (float)node.Y + 120);
                if (Distance(point, falsePos) < tolerance) return (node, true, "false");
            }
            else if (node.NodeType == NodeType.SwitchNode)
            {
                // Switch Node (Circle)
                var centerX = (float)node.X + 75;
                var centerY = (float)node.Y + 40;
                var radius = 35f;
                const double PI = Math.PI;

                // Input (Left - 180 degrees)
                var inputPos = new PointF(centerX - radius, centerY);
                if (Distance(point, inputPos) < tolerance) return (node, false, null);

                // Check Dynamic Outputs
                int count = Math.Max(2, node.OutputCount);

                double separationAngle = 30; // degrees
                double totalSpan = (count - 1) * separationAngle;
                double startAngle = -totalSpan / 2.0;

                for (int i = 0; i < count; i++)
                {
                    double angleDeg = startAngle + (separationAngle * i);
                    double angleRad = angleDeg * PI / 180.0;

                    var outPos = new PointF(
                        centerX + radius * (float)Math.Cos(angleRad),
                        centerY + radius * (float)Math.Sin(angleRad));

                    if (Distance(point, outPos) < tolerance) return (node, true, (i + 1).ToString());
                }
            }
            else
            {
                // Non-decision, non-switch nodes
                // Input: Left-Center (0, 40) relative to node
                var inputPos = new PointF((float)node.X, (float)node.Y + 40);
                if (Distance(point, inputPos) < tolerance) return (node, false, null);

                // Output: Right-Center (150, 40) relative to node
                if (node.NodeType != NodeType.OutputNode)
                {
                    var outputPos = new PointF((float)node.X + 150, (float)node.Y + 40);
                    if (Distance(point, outputPos) < tolerance) return (node, true, null);
                }
            }
        }
        return (null, false, null);
    }

    private PointF GetPortPosition(WorkflowNode node, bool isOutput, string? portType)
    {
        if (node.NodeType == NodeType.DecisionNode)
        {
            if (!isOutput) return new PointF((float)node.X + 75, (float)node.Y);
            if (portType == "true") return new PointF((float)node.X + 150, (float)node.Y + 60);
            if (portType == "false") return new PointF((float)node.X + 75, (float)node.Y + 120);
        }

        if (node.NodeType == NodeType.SwitchNode)
        {
            var centerX = (float)node.X + 75;
            var centerY = (float)node.Y + 40;
            var radius = 35f;
            const double PI = Math.PI;

            if (!isOutput) return new PointF(centerX - radius, centerY);

            if (int.TryParse(portType, out int index))
            {
                index = index - 1; // 0-based
                int count = Math.Max(2, node.OutputCount);

                if (index >= 0 && index < count)
                {
                    double separationAngle = 30; // degrees
                    double totalSpan = (count - 1) * separationAngle;
                    double startAngle = -totalSpan / 2.0;

                    double angleDeg = startAngle + (separationAngle * index);
                    double angleRad = angleDeg * PI / 180.0;

                    return new PointF(
                        centerX + radius * (float)Math.Cos(angleRad),
                        centerY + radius * (float)Math.Sin(angleRad));
                }
            }
            // Default to Center-Right if unknown
            return new PointF(centerX + radius, centerY);
        }

        if (isOutput) return new PointF((float)node.X + 150, (float)node.Y + 40);
        return new PointF((float)node.X, (float)node.Y + 40);
    }

    private float Distance(PointF p1, PointF p2)
    {
        return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler != null)
        {
            var mainViewModel = BindingContext as MainViewModel;
            var vm = mainViewModel?.WorkflowDesigner;
            if (vm != null)
            {
                vm.RequestInvalidate += (s, e) => WorkflowCanvas?.Invalidate();
            }
            WorkflowCanvas?.Invalidate();
        }
    }

    private void OnOutputCountChanged(object sender, ValueChangedEventArgs e)
    {
        WorkflowCanvas?.Invalidate();
    }
}
