using AgentDesigner.Domain.Enums;

namespace AgentDesigner.Domain.Entities;

/// <summary>
/// Represents a workflow that orchestrates agents and functions.
/// </summary>
public class Workflow
{
    /// <summary>
    /// Unique identifier for the workflow.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name of the workflow.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the workflow's purpose.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// List of nodes in the workflow.
    /// </summary>
    public List<WorkflowNode> Nodes { get; set; } = [];

    /// <summary>
    /// List of edges connecting nodes.
    /// </summary>
    public List<WorkflowEdge> Edges { get; set; } = [];

    /// <summary>
    /// Workflow version string.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Metadata tags, author, etc.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    /// <summary>
    /// Default input variables schema.
    /// </summary>
    public Dictionary<string, string> InputSchema { get; set; } = [];

    /// <summary>
    /// Output variables schema.
    /// </summary>
    public Dictionary<string, string> OutputSchema { get; set; } = [];

    /// <summary>
    /// Execution timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Maximum retry attempts for failed nodes.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modification timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a node in a workflow diagram.
/// </summary>
public partial class WorkflowNode : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    /// <summary>
    /// Unique identifier for the node.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name of the node.
    /// </summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _name = string.Empty;

    /// <summary>
    /// Description of the node.
    /// </summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _description = string.Empty;

    /// <summary>
    /// Type of the node.
    /// </summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private NodeType _nodeType;

    /// <summary>
    /// X position on the canvas.
    /// </summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private double _x;

    /// <summary>
    /// Y position on the canvas.
    /// </summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private double _y;

    /// <summary>
    /// Width of the node (for layout).
    /// </summary>
    public double Width { get; set; } = 200;

    /// <summary>
    /// Height of the node (for layout).
    /// </summary>
    public double Height { get; set; } = 100;

    /// <summary>
    /// Input port definitions.
    /// </summary>
    public List<NodePort> InputPorts { get; set; } = [];

    /// <summary>
    /// Output port definitions.
    /// </summary>
    public List<NodePort> OutputPorts { get; set; } = [];

    /// <summary>
    /// JSON configuration specific to the node type.
    /// </summary>
    public string Configuration { get; set; } = "{}";

    // Agent Node specific properties

    /// <summary>
    /// Reference to the agent (for AgentNode type).
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    /// Reference to the agent metadata ID (Integer from Database).
    /// </summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private int? _agentMetadataId;

    /// <summary>
    /// Reference to the input node metadata ID (Integer from Database).
    /// </summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private int? _inputMetadataId;

    /// <summary>
    /// Prompt template with variable placeholders (for AgentNode type).
    /// </summary>
    public string? PromptTemplate { get; set; }

    /// <summary>
    /// Mapping from workflow variables to agent inputs.
    /// </summary>
    public Dictionary<string, string>? ContextMapping { get; set; }

    // Function Node specific properties

    /// <summary>
    /// Reference to the function (for FunctionNode type).
    /// </summary>
    public Guid? FunctionId { get; set; }

    /// <summary>
    /// Mapping from workflow variables to function parameters.
    /// </summary>
    public Dictionary<string, string>? ParameterMapping { get; set; }

    /// <summary>
    /// Mapping from function return values to workflow variables.
    /// </summary>
    public Dictionary<string, string>? ReturnMapping { get; set; }

    // Decision Node specific properties

    /// <summary>
    /// Condition expression for decision nodes.
    /// </summary>
    public string? ConditionExpression { get; set; }

    // Switch Node specific properties

    /// <summary>
    /// Number of output ports for switch nodes.
    /// </summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private int _outputCount = 3;
}

/// <summary>
/// Represents a port on a workflow node for connections.
/// </summary>
public class NodePort
{
    /// <summary>
    /// Unique identifier for the port.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name of the port.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Data type of the port (for type validation).
    /// </summary>
    public string DataType { get; set; } = "any";

    /// <summary>
    /// Whether this port is required for execution.
    /// </summary>
    public bool IsRequired { get; set; }
}

/// <summary>
/// Represents an edge connecting two nodes in a workflow.
/// </summary>
public class WorkflowEdge
{
    /// <summary>
    /// Unique identifier for the edge.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID of the source node.
    /// </summary>
    public Guid SourceNodeId { get; set; }

    /// <summary>
    /// ID of the target node.
    /// </summary>
    public Guid TargetNodeId { get; set; }

    /// <summary>
    /// ID of the source port.
    /// </summary>
    public Guid SourcePortId { get; set; }

    /// <summary>
    /// ID of the target port.
    /// </summary>
    public Guid TargetPortId { get; set; }

    /// <summary>
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Label to display on the edge.
    /// </summary>
    public string? Label { get; set; }
}
