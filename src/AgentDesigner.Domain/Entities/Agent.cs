using AgentDesigner.Domain.Enums;

namespace AgentDesigner.Domain.Entities;

/// <summary>
/// Represents an AI agent that can be invoked in workflows.
/// </summary>
public class Agent
{
    /// <summary>
    /// Unique identifier for the agent.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name of the agent.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the agent's purpose and capabilities.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The type of agent (LLM, ToolAgent, Orchestrator, etc.).
    /// </summary>
    public AgentType AgentType { get; set; } = AgentType.LLM;

    /// <summary>
    /// JSON configuration for the agent (model, temperature, endpoints, etc.).
    /// </summary>
    public string Configuration { get; set; } = "{}";

    /// <summary>
    /// List of function IDs attached to this agent as tools.
    /// </summary>
    public List<Guid> AttachedFunctionIds { get; set; } = [];

    /// <summary>
    /// Metadata tags, version, author information.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    /// <summary>
    /// System prompt for the agent.
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last modification timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
