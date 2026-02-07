namespace AgentDesigner.Domain.Enums;

/// <summary>
/// Defines the compilation status of a function.
/// </summary>
public enum CompileStatus
{
    /// <summary>
    /// The function has not been compiled yet.
    /// </summary>
    NotCompiled,

    /// <summary>
    /// The function was compiled successfully.
    /// </summary>
    Compiled,

    /// <summary>
    /// Compilation failed with errors.
    /// </summary>
    Failed
}
