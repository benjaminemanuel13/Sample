using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentDesigner.Domain.Entities;
using AgentDesigner.Domain.Enums;
using AgentDesigner.UI.Services;
using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;
using System.Linq;
using AgentDesigner.Domain.Models;
using AgentDesigner.Infrastructure.Persistence;

namespace AgentDesigner.UI.ViewModels;

/// <summary>
/// View model for the workflow designer canvas.
/// </summary>
public partial class WorkflowDesignerViewModel : ObservableObject, IDrawable
{
    private readonly NavigationService _navigationService;
    private readonly SqliteMetadataRepository _metadataRepository;

    [ObservableProperty]
    private Workflow? _currentWorkflow;

    [ObservableProperty]
    private ObservableCollection<WorkflowNode> _nodes = [];

    [ObservableProperty]
    private ObservableCollection<WorkflowEdge> _edges = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNodeSelected))]
    [NotifyPropertyChangedFor(nameof(IsNotNodeSelected))]
    private WorkflowNode? _selectedNode;

    partial void OnSelectedNodeChanged(WorkflowNode? value)
    {
        if (value != null)
        {
            // Sync Picker selections from Node IDs
            SelectedAgentItem = AvailableAgents.FirstOrDefault(a => a.Id == value.AgentMetadataId);
            SelectedInputItem = AvailableInputNodes.FirstOrDefault(i => i.Id == value.InputMetadataId);
        }
        else
        {
            SelectedAgentItem = null;
            SelectedInputItem = null;
        }
    }

    public bool IsNodeSelected => SelectedNode != null;
    public bool IsNotNodeSelected => SelectedNode == null;

    [ObservableProperty]
    private double _canvasZoom = 1.0;

    [ObservableProperty]
    private double _canvasOffsetX = 0;

    [ObservableProperty]
    private double _canvasOffsetY = 0;

    // Metadata Collections
    [ObservableProperty]
    private ObservableCollection<MetadataItem> _availableAgents = [];

    [ObservableProperty]
    private ObservableCollection<MetadataItem> _availableInputNodes = [];

    // Selected Metadata Items (bound to Pickers)
    [ObservableProperty]
    private MetadataItem? _selectedAgentItem;

    partial void OnSelectedAgentItemChanged(MetadataItem? value)
    {
        if (SelectedNode != null && value != null)
        {
            SelectedNode.AgentMetadataId = value.Id;
        }
    }

    [ObservableProperty]
    private MetadataItem? _selectedInputItem;

    partial void OnSelectedInputItemChanged(MetadataItem? value)
    {
        if (SelectedNode != null && value != null)
        {
            SelectedNode.InputMetadataId = value.Id;
        }
    }

    // Temporary connection points for edge creation
    public PointF TempConnectionStart { get; set; }
    public PointF TempConnectionEnd { get; set; }

    public WorkflowDesignerViewModel(NavigationService navigationService, SqliteMetadataRepository metadataRepository)
    {
        _navigationService = navigationService;
        _metadataRepository = metadataRepository;

        LoadMetadata();
    }

    private void LoadMetadata()
    {
        var agents = _metadataRepository.GetAllAgents();
        AvailableAgents = new ObservableCollection<MetadataItem>(agents);

        var inputs = _metadataRepository.GetAllInputNodes();
        AvailableInputNodes = new ObservableCollection<MetadataItem>(inputs);
    }

    public void OnNavigatedTo()
    {
        // Get workflow from navigation service when page is navigated to
        var workflow = _navigationService.GetCurrentWorkflow();
        if (workflow != null)
        {
            LoadWorkflow(workflow);
        }
    }

    public void LoadWorkflow(Workflow workflow)
    {
        CurrentWorkflow = workflow;
        Nodes = new ObservableCollection<WorkflowNode>(workflow.Nodes);
        Edges = new ObservableCollection<WorkflowEdge>(workflow.Edges);

        // If workflow is empty, add some sample nodes so user sees something
        if (Nodes.Count == 0)
        {
            var inputNode = new WorkflowNode
            {
                Name = "Start",
                NodeType = NodeType.InputNode,
                X = 100,
                Y = 200
            };

            var outputNode = new WorkflowNode
            {
                Name = "End",
                NodeType = NodeType.OutputNode,
                X = 600,
                Y = 200
            };

            CurrentWorkflow.Nodes.Add(inputNode);
            CurrentWorkflow.Nodes.Add(outputNode);
            Nodes.Add(inputNode);
            Nodes.Add(outputNode);

            // Add edge connecting Start to End
            var edge = new WorkflowEdge
            {
                SourceNodeId = inputNode.Id,
                TargetNodeId = outputNode.Id,
                Label = "Flow"
            };

            Edges.Add(edge);
            CurrentWorkflow.Edges.Add(edge);
        }
    }

    [RelayCommand]
    private void AddAgentNode()
    {
        if (CurrentWorkflow == null) return;

        var node = new WorkflowNode
        {
            Name = "New Agent",
            NodeType = NodeType.AgentNode,
            X = 100,
            Y = 100
        };

        CurrentWorkflow.Nodes.Add(node);
        Nodes.Add(node);
    }

    [RelayCommand]
    private void AddFunctionNode()
    {
        if (CurrentWorkflow == null) return;

        var node = new WorkflowNode
        {
            Name = "Function",
            NodeType = NodeType.FunctionNode,
            X = 300 + Nodes.Count * 50,
            Y = 100 + Nodes.Count * 50
        };
        Nodes.Add(node);
        CurrentWorkflow.Nodes.Add(node);
    }

    [RelayCommand]
    private void AddDecisionNode()
    {
        if (CurrentWorkflow == null)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: CurrentWorkflow is null in AddDecisionNode");
            return;
        }

        var node = new WorkflowNode
        {
            Name = "Decision",
            NodeType = NodeType.DecisionNode,
            X = 300 + Nodes.Count * 50,
            Y = 100 + Nodes.Count * 50,
            ConditionExpression = "value > 0"
        };
        Nodes.Add(node);
        CurrentWorkflow.Nodes.Add(node);
        System.Diagnostics.Debug.WriteLine($"Added DecisionNode. UI Nodes: {Nodes.Count}, Model Nodes: {CurrentWorkflow.Nodes.Count}");
    }

    [RelayCommand]
    private void AddInputNode()
    {
        if (CurrentWorkflow == null) return;

        var node = new WorkflowNode
        {
            Name = "Input",
            NodeType = NodeType.InputNode,
            X = 300 + Nodes.Count * 50,
            Y = 100 + Nodes.Count * 50
        };
        Nodes.Add(node);
        CurrentWorkflow.Nodes.Add(node);
    }

    [RelayCommand]
    private void AddOutputNode()
    {
        if (CurrentWorkflow == null) return;

        var node = new WorkflowNode
        {
            Name = "Output",
            NodeType = NodeType.OutputNode,
            X = 500,
            Y = 200
        };

        CurrentWorkflow.Nodes.Add(node);
        Nodes.Add(node);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExecuting))]
    private bool _isWorkflowExecuting;

    public bool IsExecuting => IsWorkflowExecuting;

    private readonly HashSet<Guid> _executingNodeIds = [];

    public event EventHandler? RequestInvalidate;

    private void Invalidate()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            RequestInvalidate?.Invoke(this, EventArgs.Empty);
        });
    }

    [RelayCommand]
    private async Task CompileWorkflow()
    {
        if (CurrentWorkflow == null || IsWorkflowExecuting) return;

        IsWorkflowExecuting = true;
        _executingNodeIds.Clear();

        // Find Start Node (InputNode)
        var startNode = Nodes.FirstOrDefault(n => n.NodeType == NodeType.InputNode);
        if (startNode == null)
        {
            await Shell.Current.DisplayAlert("Error", "No Input Node found to start execution.", "OK");
            IsWorkflowExecuting = false;
            return;
        }

        await ExecuteNode(startNode);

        _executingNodeIds.Clear();
        IsWorkflowExecuting = false;
        Invalidate();

        await Shell.Current.DisplayAlert("Create", "Execution Finished", "OK");
    }

    private async Task ExecuteNode(WorkflowNode node)
    {
        // Highlight current node
        lock (_executingNodeIds)
        {
            _executingNodeIds.Add(node.Id);
        }
        Invalidate(); // Trigger redraw

        // Wait 1 second
        await Task.Delay(1000);

        // Remove highlight before moving on
        lock (_executingNodeIds)
        {
            _executingNodeIds.Remove(node.Id);
        }
        Invalidate();

        // Find next node(s)
        var outgoingEdges = Edges.Where(e => e.SourceNodeId == node.Id).ToList();

        if (outgoingEdges.Any())
        {
            var nextNodes = outgoingEdges
                .Select(e => Nodes.FirstOrDefault(n => n.Id == e.TargetNodeId))
                .Where(n => n != null)
                .Cast<WorkflowNode>()
                .ToList();

            if (nextNodes.Any())
            {
                // Parallel Execution: Await all branches efficiently
                await Task.WhenAll(nextNodes.Select(n => ExecuteNode(n)));
            }
        }
    }

    [RelayCommand]
    private void DeleteNode()
    {
        if (CurrentWorkflow == null || SelectedNode == null) return;

        CurrentWorkflow.Nodes.Remove(SelectedNode);
        Nodes.Remove(SelectedNode);

        // Remove connected edges
        var connectedEdges = CurrentWorkflow.Edges
            .Where(e => e.SourceNodeId == SelectedNode.Id || e.TargetNodeId == SelectedNode.Id)
            .ToList();

        foreach (var edge in connectedEdges)
        {
            CurrentWorkflow.Edges.Remove(edge);
            Edges.Remove(edge);
        }

        SelectedNode = null;
    }

    [RelayCommand]
    private void ZoomIn()
    {
        CanvasZoom = Math.Min(CanvasZoom * 1.2, 3.0);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        CanvasZoom = Math.Max(CanvasZoom / 1.2, 0.3);
    }

    [RelayCommand]
    private void ResetZoom()
    {
        CanvasZoom = 1.0;
        CanvasOffsetX = 0;
        CanvasOffsetY = 0;
    }

    // IDrawable implementation for GraphicsView
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Color.FromArgb("#F8F9FA");
        canvas.FillRectangle(dirtyRect);

        canvas.SaveState();
        canvas.Translate((float)CanvasOffsetX, (float)CanvasOffsetY);
        canvas.Scale((float)CanvasZoom, (float)CanvasZoom);

        // Draw edges first (so they appear behind nodes)
        foreach (var edge in Edges)
        {
            DrawEdge(canvas, edge);
        }

        // Draw each node
        foreach (var node in Nodes)
        {
            DrawNode(canvas, node);
        }

        // Draw temporary connection if active
        if (TempConnectionStart != PointF.Zero && TempConnectionEnd != PointF.Zero)
        {
            DrawTempConnection(canvas);
        }

        canvas.RestoreState();
    }

    private void DrawTempConnection(ICanvas canvas)
    {
        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 2;
        canvas.StrokeDashPattern = new float[] { 5, 5 }; // Dashed line

        canvas.DrawLine(TempConnectionStart, TempConnectionEnd);

        // Reset stroke settings
        canvas.StrokeDashPattern = null;
    }

    private void DrawNode(ICanvas canvas, WorkflowNode node)
    {
        if (node.NodeType == NodeType.DecisionNode)
        {
            DrawDecisionNode(canvas, node);
        }
        else if (node.NodeType == NodeType.SwitchNode)
        {
            DrawSwitchNodeCircle(canvas, node);
        }
        else
        {
            DrawRegularNode(canvas, node);
        }
    }

    private void DrawRegularNode(ICanvas canvas, WorkflowNode node)
    {
        var rect = new RectF((float)node.X, (float)node.Y, 150, 80);

        // Draw shadow
        canvas.FillColor = Colors.Black.WithAlpha(0.1f);
        canvas.FillRoundedRectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height, 4);

        // Draw node background
        canvas.FillColor = Colors.White;
        canvas.FillRoundedRectangle(rect, 4);

        // Draw border
        bool isExecuting;
        lock (_executingNodeIds) { isExecuting = _executingNodeIds.Contains(node.Id); }

        if (isExecuting)
        {
            canvas.StrokeColor = Colors.Lime; // Execution Highlight
            canvas.StrokeSize = 4;
        }
        else
        {
            canvas.StrokeColor = node == SelectedNode ? Colors.Orange : Colors.Blue;
            canvas.StrokeSize = 2;
        }
        canvas.DrawRoundedRectangle(rect, 4);

        // Draw input port (left side) - only if not an InputNode
        if (node.NodeType != NodeType.InputNode)
        {
            var inputPort = GetInputPortPosition(node);
            canvas.FillColor = Colors.Green;
            canvas.FillCircle(inputPort.X, inputPort.Y, 6);
            canvas.StrokeColor = Colors.DarkGreen;
            canvas.StrokeSize = 2;
            canvas.DrawCircle(inputPort.X, inputPort.Y, 6);
        }

        // Draw output port (right side) - only if not an OutputNode
        if (node.NodeType != NodeType.OutputNode)
        {
            var outputPort = GetOutputPortPosition(node);
            canvas.FillColor = Colors.Red;
            canvas.FillCircle(outputPort.X, outputPort.Y, 6);
            canvas.StrokeColor = Colors.DarkRed;
            canvas.StrokeSize = 2;
            canvas.DrawCircle(outputPort.X, outputPort.Y, 6);
        }

        // Draw text
        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 10;
        canvas.DrawString(node.NodeType.ToString(), rect.X + 10, rect.Y + 15, HorizontalAlignment.Left);

        canvas.FontColor = Colors.Black;
        canvas.FontSize = 14;
        canvas.DrawString(node.Name, rect.X + 10, rect.Y + 35, HorizontalAlignment.Left);

        canvas.FontColor = Colors.LightGray;
        canvas.FontSize = 8;
        canvas.DrawString($"({node.X:F0}, {node.Y:F0})", rect.X + 10, rect.Y + 55, HorizontalAlignment.Left);
    }

    private void DrawDecisionNode(ICanvas canvas, WorkflowNode node)
    {
        var centerX = (float)(node.X + 75);
        var centerY = (float)(node.Y + 60);
        var width = 150f;
        var height = 120f;

        // Create diamond path
        var path = new PathF();
        path.MoveTo(centerX, centerY - height / 2); // Top
        path.LineTo(centerX + width / 2, centerY); // Right
        path.LineTo(centerX, centerY + height / 2); // Bottom
        path.LineTo(centerX - width / 2, centerY); // Left
        path.Close();

        // Draw shadow
        canvas.FillColor = Colors.Black.WithAlpha(0.1f);
        var shadowPath = new PathF();
        shadowPath.MoveTo(centerX + 2, centerY - height / 2 + 2);
        shadowPath.LineTo(centerX + width / 2 + 2, centerY + 2);
        shadowPath.LineTo(centerX + 2, centerY + height / 2 + 2);
        shadowPath.LineTo(centerX - width / 2 + 2, centerY + 2);
        shadowPath.Close();
        canvas.FillPath(shadowPath);

        // Draw background
        canvas.FillColor = Color.FromArgb("#FFF9E6");
        canvas.FillPath(path);

        // Draw border
        bool isExecuting;
        lock (_executingNodeIds) { isExecuting = _executingNodeIds.Contains(node.Id); }

        if (isExecuting)
        {
            canvas.StrokeColor = Colors.Lime; // Execution Highlight
            canvas.StrokeSize = 4;
        }
        else
        {
            canvas.StrokeColor = node == SelectedNode ? Colors.Orange : Color.FromArgb("#FFA500");
            canvas.StrokeSize = 2;
        }
        canvas.DrawPath(path);

        // Draw input port (top)
        var inputPort = new PointF(centerX, centerY - height / 2);
        canvas.FillColor = Colors.Green;
        canvas.FillCircle(inputPort.X, inputPort.Y, 6);
        canvas.StrokeColor = Colors.DarkGreen;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(inputPort.X, inputPort.Y, 6);

        // Draw TRUE output port (right)
        var truePort = new PointF(centerX + width / 2, centerY);
        canvas.FillColor = Colors.LightGreen;
        canvas.FillCircle(truePort.X, truePort.Y, 6);
        canvas.StrokeColor = Colors.Green;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(truePort.X, truePort.Y, 6);
        canvas.FontColor = Colors.Green;
        canvas.FontSize = 10;
        canvas.DrawString("T", truePort.X + 10, truePort.Y + 5, HorizontalAlignment.Left);

        // Draw FALSE output port (bottom)
        var falsePort = new PointF(centerX, centerY + height / 2);
        canvas.FillColor = Colors.LightCoral;
        canvas.FillCircle(falsePort.X, falsePort.Y, 6);
        canvas.StrokeColor = Colors.Red;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(falsePort.X, falsePort.Y, 6);
        canvas.FontColor = Colors.Red;
        canvas.FontSize = 10;
        canvas.DrawString("F", falsePort.X + 5, falsePort.Y + 15, HorizontalAlignment.Left);

        // Draw text in center
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 12;
        canvas.DrawString(node.Name, centerX, centerY - 10, HorizontalAlignment.Center);

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 9;
        var condition = node.ConditionExpression ?? "condition";
        canvas.DrawString(condition, centerX, centerY + 10, HorizontalAlignment.Center);
    }

    private PointF GetInputPortPosition(WorkflowNode node)
    {
        if (node.NodeType == NodeType.DecisionNode)
        {
            // Top of diamond
            return new PointF((float)(node.X + 75), (float)(node.Y));
        }
        else if (node.NodeType == NodeType.SwitchNode)
        {
            // Left vertex of circle
            var centerX = (float)(node.X + 75);
            var centerY = (float)(node.Y + 40); // Centered in standard node height
            var radius = 35f;
            return new PointF(centerX - radius, centerY);
        }
        // Left side, centered vertically
        return new PointF((float)node.X, (float)(node.Y + 40));
    }

    private PointF GetOutputPortPosition(WorkflowNode node)
    {
        if (node.NodeType == NodeType.DecisionNode)
        {
            // Right of diamond (TRUE path)
            return new PointF((float)(node.X + 150), (float)(node.Y + 60));
        }
        // Right side, centered vertically
        return new PointF((float)(node.X + 150), (float)(node.Y + 40));
    }

    private PointF GetDecisionFalsePortPosition(WorkflowNode node)
    {
        // Bottom of diamond (FALSE path)
        return new PointF((float)(node.X + 75), (float)(node.Y + 120));
    }

    private void DrawEdge(ICanvas canvas, WorkflowEdge edge)
    {
        var sourceNode = Nodes.FirstOrDefault(n => n.Id == edge.SourceNodeId);
        var targetNode = Nodes.FirstOrDefault(n => n.Id == edge.TargetNodeId);

        if (sourceNode != null && targetNode != null)
        {
            // Get port positions based on node type and edge label
            PointF sourcePort;

            // For decision nodes, check the edge label to determine which port to use
            if (sourceNode.NodeType == NodeType.DecisionNode)
            {
                if (edge.Label == "false")
                {
                    // Use FALSE port (bottom)
                    sourcePort = GetDecisionFalsePortPosition(sourceNode);
                }
                else
                {
                    // Use TRUE port (right) - default for decision nodes
                    sourcePort = GetOutputPortPosition(sourceNode);
                }
            }
            else if (sourceNode.NodeType == NodeType.SwitchNode)
            {
                sourcePort = GetSwitchPortPosition(sourceNode, edge.Label);
            }
            else
            {
                // Regular node output port
                sourcePort = GetOutputPortPosition(sourceNode);
            }

            var targetPort = GetInputPortPosition(targetNode);

            // Draw bezier curve for smoother connection
            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 2;

            var path = new PathF();
            path.MoveTo(sourcePort);

            // Calculate control points for bezier curve
            var controlPoint1 = new PointF(sourcePort.X + 50, sourcePort.Y);
            var controlPoint2 = new PointF(targetPort.X - 50, targetPort.Y);

            path.CurveTo(controlPoint1, controlPoint2, targetPort);
            canvas.DrawPath(path);

            // Draw arrowhead at target
            DrawArrowhead(canvas, controlPoint2, targetPort);
        }
    }

    private void DrawArrowhead(ICanvas canvas, PointF from, PointF to)
    {
        // Calculate angle
        var angle = Math.Atan2(to.Y - from.Y, to.X - from.X);
        var arrowSize = 8;

        // Calculate arrowhead points
        var point1 = new PointF(
            to.X - arrowSize * (float)Math.Cos(angle - Math.PI / 6),
            to.Y - arrowSize * (float)Math.Sin(angle - Math.PI / 6)
        );
        var point2 = new PointF(
            to.X - arrowSize * (float)Math.Cos(angle + Math.PI / 6),
            to.Y - arrowSize * (float)Math.Sin(angle + Math.PI / 6)
        );

        // Draw filled triangle
        var path = new PathF();
        path.MoveTo(to);
        path.LineTo(point1);
        path.LineTo(point2);
        path.Close();

        canvas.FillColor = Colors.Gray;
        canvas.FillPath(path);
    }

    public void Connect(Guid sourceNodeId, Guid targetNodeId, Guid sourcePortId, Guid targetPortId)
    {
        if (CurrentWorkflow == null) return;

        // Check if edge already exists
        if (CurrentWorkflow.Edges.Any(e => e.SourceNodeId == sourceNodeId && e.TargetNodeId == targetNodeId))
        {
            return;
        }

        var sourceNode = Nodes.FirstOrDefault(n => n.Id == sourceNodeId);
        var edgeLabel = "";

        // Determine label for decision nodes
        if (sourceNode?.NodeType == NodeType.DecisionNode)
        {
            // If the source port is the FALSE port (bottom), label it "false"
            // We rely on the fact that MainView logic determined the port correctly.
            // But we don't have the portType here explicitly in arguments, only IDs.
            // However, we can infer it if we check the port ID against the node's ports if they were defined in the model.
            // But we render ports dynamically in Draw method without storing them in a list in the model yet (except implicit Lists in Node).
            // Wait, WorkflowNode has InputPorts/OutputPorts lists in Domain!
            // But Draw method uses Get...PortPosition() dynamic calculation, and doesn't seemingly populate the lists?
            // Let's check AddDecisionNode. It creates a node but doesn't populate ports list?
            // If ports list is empty, we can't look up by ID.
            // MainView's FindPortAtPoint logic uses GetPortPosition geometry check, it doesn't return a Port object from the model, it returns the Node and type.
            // MainView Connect call uses `_connectionSourceNode?.OutputPorts.FirstOrDefault()?.Id`. 
            // If OutputPorts is empty, Id is empty/null access? FirstOrDefault() returns null. ?.Id returns null? No, Guid struct?
            // WorkflowNode.OutputPorts is List<NodePort>. FirstOrDefault returns NodePort object.
            // If list is empty, returns null. ?.Id -> null.
            // Guid.Empty used in MainView call: `?? Guid.Empty`.

            // So we are passing Guid.Empty if the model ports aren't populated.
            // This might mean edges don't track ports correctly yet, but for now we just want it to build.
        }

        var edge = new WorkflowEdge
        {
            SourceNodeId = sourceNodeId,
            TargetNodeId = targetNodeId,
            SourcePortId = sourcePortId,
            TargetPortId = targetPortId,
            Label = edgeLabel
        };

        CurrentWorkflow.Edges.Add(edge);
        Edges.Add(edge);
    }

    public PointF GetPortPosition(WorkflowNode node, bool isOutput, string? portType)
    {
        if (node.NodeType == NodeType.DecisionNode)
        {
            if (isOutput)
            {
                if (portType == "false") return GetDecisionFalsePortPosition(node);
                return GetOutputPortPosition(node); // True path
            }
            return GetInputPortPosition(node);
        }
        else if (node.NodeType == NodeType.SwitchNode)
        {
            if (isOutput)
            {
                return GetSwitchPortPosition(node, portType);
            }
            return GetInputPortPosition(node);
        }

        if (isOutput) return GetOutputPortPosition(node);
        return GetInputPortPosition(node);
    }

    private PointF GetSwitchPortPosition(WorkflowNode node, string? portLabel)
    {
        var centerX = (float)(node.X + 75);
        var centerY = (float)(node.Y + 40);
        var radius = 35f;

        if (int.TryParse(portLabel, out int index))
        {
            // Adjust to 0-based index
            index = index - 1;

            int count = Math.Max(2, node.OutputCount);
            if (index >= 0 && index < count)
            {
                double separationAngle = 30; // degrees
                double totalSpan = (count - 1) * separationAngle;
                double startAngle = -totalSpan / 2.0;

                double angleDeg = startAngle + (separationAngle * index);
                double angleRad = angleDeg * Math.PI / 180.0;

                return new PointF(
                    centerX + radius * (float)Math.Cos(angleRad),
                    centerY + radius * (float)Math.Sin(angleRad));
            }
        }

        // Default to Right Center if something fails
        return new PointF(centerX + radius, centerY);
    }

    [RelayCommand]
    private void AddSwitchNode()
    {
        if (CurrentWorkflow == null) return;

        var node = new WorkflowNode
        {
            Name = "Switch",
            NodeType = NodeType.SwitchNode,
            X = 300 + Nodes.Count * 50,
            Y = 100 + Nodes.Count * 50
        };
        Nodes.Add(node);
        CurrentWorkflow.Nodes.Add(node);
    }

    private void DrawSwitchNode(ICanvas canvas, WorkflowNode node)
    {
        var centerX = (float)(node.X + 75);
        var centerY = (float)(node.Y + 60);
        var radius = 70f;

        // Create Hexagon Path
        var path = new PathF();
        for (int i = 0; i < 6; i++)
        {
            float angle_deg = 60 * i;
            float angle_rad = (float)(Math.PI / 180f * angle_deg);
            float px = centerX + radius * (float)Math.Cos(angle_rad);
            float py = centerY + radius * (float)Math.Sin(angle_rad);

            if (i == 0) path.MoveTo(px, py);
            else path.LineTo(px, py);
        }
        path.Close();

        // Draw shadow
        canvas.FillColor = Colors.Black.WithAlpha(0.1f);
        canvas.FillPath(path); // Simplified shadow for now

        // Draw background
        canvas.FillColor = Color.FromArgb("#E1BEE7"); // Light Purple
        canvas.FillPath(path);

        // Draw border
        canvas.StrokeColor = node == SelectedNode ? Colors.Orange : Colors.Purple;
        canvas.StrokeSize = 2;
        canvas.DrawPath(path);

        // Input Port (Left)
        var inputPort = new PointF(centerX - radius, centerY);
        canvas.FillColor = Colors.Green;
        canvas.FillCircle(inputPort.X, inputPort.Y, 6);
        canvas.StrokeColor = Colors.DarkGreen;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(inputPort.X, inputPort.Y, 6);

        // Output Ports (3 on the right side)
        // We'll place them at angles -30, 0, 30? Or on the vertices?
        // Hexagon vertices at 0 (Right), 60 (Bottom Right), 300 (Top Right)

        var out1 = new PointF(centerX + radius * (float)Math.Cos(Math.PI / 180 * 300), centerY + radius * (float)Math.Sin(Math.PI / 180 * 300)); // Top Right
        var out2 = new PointF(centerX + radius, centerY); // Right
        var out3 = new PointF(centerX + radius * (float)Math.Cos(Math.PI / 180 * 60), centerY + radius * (float)Math.Sin(Math.PI / 180 * 60)); // Bottom Right

        DrawSwitchPort(canvas, out1, "1");
        DrawSwitchPort(canvas, out2, "2");
        DrawSwitchPort(canvas, out3, "3");

        // Draw text
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 12;
        canvas.DrawString(node.Name, centerX, centerY, HorizontalAlignment.Center);
    }

    private void DrawSwitchPort(ICanvas canvas, PointF pos, string label)
    {
        canvas.FillColor = Colors.Red;
        canvas.FillCircle(pos.X, pos.Y, 6);
        canvas.StrokeColor = Colors.DarkRed;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(pos.X, pos.Y, 6);

        canvas.FontColor = Colors.Purple;
        canvas.FontSize = 10;
        canvas.DrawString(label, pos.X + 10, pos.Y, HorizontalAlignment.Left);
    }

    private void DrawSwitchNodeCircle(ICanvas canvas, WorkflowNode node)
    {
        var centerX = (float)(node.X + 75);
        var centerY = (float)(node.Y + 60); // Centered in 150x120 area effectively? No, Node is at X,Y
                                            // Node size for hit testing was huge. Let's make it smaller.
                                            // Standard node is 150x80.
                                            // Let's center it in standard node area: X+75, Y+40

        centerX = (float)(node.X + 75);
        centerY = (float)(node.Y + 40);

        // Radius was 70. Let's make it smaller. 
        // Height of standard node is 80. Radius 40 fits exactly.
        // "Slightly smaller" -> Radius 35.
        var radius = 35f;

        // Draw shadow
        canvas.FillColor = Colors.Black.WithAlpha(0.1f);
        canvas.FillCircle(centerX + 2, centerY + 2, radius);

        // Draw background (Standard White)
        canvas.FillColor = Colors.White;
        canvas.FillCircle(centerX, centerY, radius);

        // Draw border
        bool isExecuting;
        lock (_executingNodeIds) { isExecuting = _executingNodeIds.Contains(node.Id); }

        if (isExecuting)
        {
            canvas.StrokeColor = Colors.Lime; // Execution Highlight
            canvas.StrokeSize = 4;
        }
        else
        {
            canvas.StrokeColor = node == SelectedNode ? Colors.Orange : Colors.Purple;
            canvas.StrokeSize = 2;
        }
        canvas.DrawCircle(centerX, centerY, radius);

        // Input Port (Left - 180 degrees)
        var inputPort = new PointF(centerX - radius, centerY);
        canvas.FillColor = Colors.Green;
        canvas.FillCircle(inputPort.X, inputPort.Y, 6);
        canvas.StrokeColor = Colors.DarkGreen;
        canvas.StrokeSize = 2;
        canvas.DrawCircle(inputPort.X, inputPort.Y, 6);

        // Output Ports (Distributed on right side)
        int count = Math.Max(2, node.OutputCount);

        // Constant distance means constant angle step?
        // Let's use 30 degrees separation.
        // Center is 0 degrees.
        // Range is (count - 1) * 30 degrees.
        // Start angle = - (Range / 2)

        double separationAngle = 30; // degrees
        double totalSpan = (count - 1) * separationAngle;
        double startAngle = -totalSpan / 2.0;

        for (int i = 0; i < count; i++)
        {
            double angleDeg = startAngle + (separationAngle * i);
            double angleRad = angleDeg * Math.PI / 180.0;

            float px = centerX + radius * (float)Math.Cos(angleRad);
            float py = centerY + radius * (float)Math.Sin(angleRad);

            DrawSwitchPort(canvas, new PointF(px, py), (i + 1).ToString());
        }

        // Draw text
        canvas.FontColor = Colors.Black;
        canvas.FontSize = 10;
        canvas.DrawString(node.Name, centerX, centerY, HorizontalAlignment.Center);
    }
}
