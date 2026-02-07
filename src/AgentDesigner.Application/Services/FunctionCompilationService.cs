using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using AgentDesigner.Domain.Entities;
using AgentDesigner.Domain.Enums;
using System.Reflection;

namespace AgentDesigner.Application.Services;

/// <summary>
/// Result of a compilation operation.
/// </summary>
public class CompilationResult
{
    public bool Success { get; set; }
    public string? AssemblyPath { get; set; }
    public List<CompileDiagnostic> Diagnostics { get; set; } = [];
}

/// <summary>
/// Service for compiling C# functions using Roslyn.
/// </summary>
public class FunctionCompilationService
{
    private readonly string _outputDirectory;

    public FunctionCompilationService(string? outputDirectory = null)
    {
        _outputDirectory = outputDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AgentDesigner",
            "Assemblies");

        Directory.CreateDirectory(_outputDirectory);
    }

    /// <summary>
    /// Compiles a function's source code into a .NET assembly.
    /// </summary>
    public async Task<CompilationResult> CompileAsync(Function function)
    {
        var result = new CompilationResult();

        try
        {
            // Parse the source code
            var syntaxTree = CSharpSyntaxTree.ParseText(function.SourceCode);

            // Assembly name
            var assemblyName = $"Function_{function.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            // Get references to required assemblies
            var references = GetMetadataReferences();

            // Create compilation
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    allowUnsafe: false));

            // Output path
            var outputPath = Path.Combine(_outputDirectory, $"{assemblyName}.dll");

            // Emit the assembly
            EmitResult emitResult;
            await using (var stream = File.Create(outputPath))
            {
                emitResult = compilation.Emit(stream);
            }

            // Process diagnostics
            result.Diagnostics = emitResult.Diagnostics
                .Where(d => d.Severity >= DiagnosticSeverity.Warning)
                .Select(d => new CompileDiagnostic
                {
                    Severity = d.Severity.ToString(),
                    Code = d.Id,
                    Message = d.GetMessage(),
                    Line = d.Location.GetLineSpan().StartLinePosition.Line + 1,
                    Column = d.Location.GetLineSpan().StartLinePosition.Character + 1
                })
                .ToList();

            if (emitResult.Success)
            {
                result.Success = true;
                result.AssemblyPath = outputPath;

                // Update function entity
                function.AssemblyPath = outputPath;
                function.CompileStatus = CompileStatus.Compiled;
                function.LastCompiledAt = DateTime.UtcNow;
                function.Diagnostics = result.Diagnostics;
            }
            else
            {
                result.Success = false;
                function.CompileStatus = CompileStatus.Failed;
                function.Diagnostics = result.Diagnostics;

                // Clean up failed assembly
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Diagnostics.Add(new CompileDiagnostic
            {
                Severity = "Error",
                Code = "COMPILE_ERROR",
                Message = $"Compilation failed: {ex.Message}",
                Line = 0,
                Column = 0
            });

            function.CompileStatus = CompileStatus.Failed;
            function.Diagnostics = result.Diagnostics;
        }

        return result;
    }

    /// <summary>
    /// Gets the metadata references for compilation.
    /// </summary>
    private static List<MetadataReference> GetMetadataReferences()
    {
        var references = new List<MetadataReference>();

        // Core .NET assemblies
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Console).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Linq.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Threading.Tasks.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll")));

        return references;
    }

    /// <summary>
    /// Validates function source code without compiling.
    /// </summary>
    public List<CompileDiagnostic> ValidateSourceCode(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            "ValidationCompilation",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return compilation.GetDiagnostics()
            .Where(d => d.Severity >= DiagnosticSeverity.Warning)
            .Select(d => new CompileDiagnostic
            {
                Severity = d.Severity.ToString(),
                Code = d.Id,
                Message = d.GetMessage(),
                Line = d.Location.GetLineSpan().StartLinePosition.Line + 1,
                Column = d.Location.GetLineSpan().StartLinePosition.Character + 1
            })
            .ToList();
    }
}
