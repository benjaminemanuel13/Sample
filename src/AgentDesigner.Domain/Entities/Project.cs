namespace AgentDesigner.Domain.Entities;

/// <summary>
/// Represents a project containing agents, functions, and workflows.
/// </summary>
public class Project
{
    /// <summary>
    /// Unique identifier for the project.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name of the project.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the project.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// File path where the project is saved.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// List of agents in the project.
    /// </summary>
    public List<Agent> Agents { get; set; } = [];

    /// <summary>
    /// List of functions in the project.
    /// </summary>
    public List<Function> Functions { get; set; } = [];

    /// <summary>
    /// List of workflows in the project.
    /// </summary>
    public List<Workflow> Workflows { get; set; } = [];

    /// <summary>
    /// Project-level settings as JSON.
    /// </summary>
    public string Settings { get; set; } = "{}";

    /// <summary>
    /// Project version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modification timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
