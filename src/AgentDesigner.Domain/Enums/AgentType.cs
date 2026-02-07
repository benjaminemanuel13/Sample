namespace AgentDesigner.Domain.Enums;

/// <summary>
/// Defines the type of AI agent.
/// </summary>
public enum AgentType
{
    /// <summary>
    /// A large language model agent that processes natural language.
    /// </summary>
    LLM,
    
    /// <summary>
    /// An agent that primarily executes tools/functions.
    /// </summary>
    ToolAgent,
    
    /// <summary>
    /// An orchestrating agent that coordinates other agents.
    /// </summary>
    Orchestrator,
    
    /// <summary>
    /// A custom agent type.
    /// </summary>
    Custom
}
