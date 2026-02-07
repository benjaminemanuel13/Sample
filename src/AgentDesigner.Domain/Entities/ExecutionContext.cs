using AgentDesigner.Domain.Enums;

namespace AgentDesigner.Domain.Entities;

/// <summary>
/// Represents the runtime context during workflow execution.
/// </summary>
public class ExecutionContext
{
    /// <summary>
    /// Unique identifier for this execution.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID of the workflow being executed.
    /// </summary>
    public Guid WorkflowId { get; set; }

    /// <summary>
    /// Workflow variables (key-value store).
    /// </summary>
    public Dictionary<string, object?> Variables { get; set; } = [];

    /// <summary>
    /// Node execution states.
    /// </summary>
    public Dictionary<Guid, NodeExecutionState> NodeStates { get; set; } = [];

    /// <summary>
    /// Execution logs.
    /// </summary>
    public List<ExecutionLogEntry> Logs { get; set; } = [];

    /// <summary>
    /// Whether the execution is currently running.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Whether the execution was cancelled.
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// Start time of the execution.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// End time of the execution (if completed).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Error message if execution failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Cancellation token source for stopping execution.
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { get; set; } = new();
}

/// <summary>
/// Represents a log entry during workflow execution.
/// </summary>
public class ExecutionLogEntry
{
    /// <summary>
    /// Timestamp of the log entry.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Log level (Info, Warning, Error).
    /// </summary>
    public string Level { get; set; } = "Info";

    /// <summary>
    /// ID of the node that generated this log (if applicable).
    /// </summary>
    public Guid? NodeId { get; set; }

    /// <summary>
    /// The log message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional detailed data (e.g., agent response, function result).
    /// </summary>
    public string? Data { get; set; }
}
