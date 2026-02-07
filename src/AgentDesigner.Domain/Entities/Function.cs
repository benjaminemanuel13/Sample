using AgentDesigner.Domain.Enums;

namespace AgentDesigner.Domain.Entities;

/// <summary>
/// Represents a user-authored function that can be compiled and executed.
/// </summary>
public class Function
{
    /// <summary>
    /// Unique identifier for the function.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name of the function.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what the function does.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The C# source code of the function.
    /// </summary>
    public string SourceCode { get; set; } = string.Empty;

    /// <summary>
    /// Fully qualified entry point (e.g., "MyNamespace.MyClass.Execute").
    /// </summary>
    public string EntryPoint { get; set; } = string.Empty;

    /// <summary>
    /// Path to the compiled assembly (DLL).
    /// </summary>
    public string? AssemblyPath { get; set; }

    /// <summary>
    /// C# language version used for compilation.
    /// </summary>
    public string LanguageVersion { get; set; } = "12.0";

    /// <summary>
    /// Timestamp of last successful compilation.
    /// </summary>
    public DateTime? LastCompiledAt { get; set; }

    /// <summary>
    /// Current compilation status.
    /// </summary>
    public CompileStatus CompileStatus { get; set; } = CompileStatus.NotCompiled;

    /// <summary>
    /// Compiler diagnostics (errors and warnings).
    /// </summary>
    public List<CompileDiagnostic> Diagnostics { get; set; } = [];

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
/// Represents a compiler diagnostic message.
/// </summary>
public class CompileDiagnostic
{
    /// <summary>
    /// Diagnostic severity (Error, Warning, Info).
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Diagnostic code (e.g., CS0001).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The diagnostic message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the diagnostic occurred.
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Column number where the diagnostic occurred.
    /// </summary>
    public int Column { get; set; }
}
