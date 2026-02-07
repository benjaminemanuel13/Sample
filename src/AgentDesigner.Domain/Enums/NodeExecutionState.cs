namespace AgentDesigner.Domain.Enums;

/// <summary>
/// Defines the execution state of a workflow node.
/// </summary>
public enum NodeExecutionState
{
    /// <summary>
    /// The node is waiting to be executed.
    /// </summary>
    Pending,

    /// <summary>
    /// The node is currently executing.
    /// </summary>
    Running,

    /// <summary>
    /// The node completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The node failed during execution.
    /// </summary>
    Failed,

    /// <summary>
    /// The node was skipped (e.g., due to conditional routing).
    /// </summary>
    Skipped
}
