namespace AgentDesigner.Domain.Enums;

/// <summary>
/// Defines the type of workflow node.
/// </summary>
public enum NodeType
{
    /// <summary>
    /// A node that invokes an AI agent.
    /// </summary>
    AgentNode,

    /// <summary>
    /// A node that executes a compiled function.
    /// </summary>
    FunctionNode,

    /// <summary>
    /// A node for conditional branching based on expressions.
    /// </summary>
    DecisionNode,

    /// <summary>
    /// A node that accepts user or external input.
    /// </summary>
    InputNode,

    /// <summary>
    /// A node that produces workflow output.
    /// </summary>
    OutputNode,

    /// <summary>
    /// A node that executes child nodes in parallel.
    /// </summary>
    ParallelNode,

    /// <summary>
    /// A node that invokes a sub-workflow.
    /// </summary>
    SubWorkflowNode,

    /// <summary>
    /// A node that branches execution into multiple paths.
    /// </summary>
    SwitchNode
}
