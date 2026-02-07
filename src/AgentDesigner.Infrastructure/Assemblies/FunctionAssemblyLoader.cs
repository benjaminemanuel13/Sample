using System.Reflection;
using System.Runtime.Loader;
using AgentDesigner.Domain.Entities;

namespace AgentDesigner.Infrastructure.Assemblies;

/// <summary>
/// Custom AssemblyLoadContext for loading function assemblies with isolation.
/// Supports unloading for recompilation scenarios.
/// </summary>
public class FunctionLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver? _resolver;

    public FunctionLoadContext(string name, string? assemblyPath = null)
        : base(name, isCollectible: true)
    {
        if (assemblyPath != null)
        {
            _resolver = new AssemblyDependencyResolver(assemblyPath);
        }
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (_resolver != null)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
        }

        return null;
    }
}

/// <summary>
/// Manages loading, unloading, and invocation of function assemblies.
/// </summary>
public class FunctionAssemblyLoader : IDisposable
{
    private readonly Dictionary<Guid, (FunctionLoadContext Context, MethodInfo Method)> _loadedFunctions = [];
    private readonly string _assembliesDirectory;
    private bool _disposed;

    public FunctionAssemblyLoader(string? assembliesDirectory = null)
    {
        _assembliesDirectory = assembliesDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AgentDesigner",
            "Assemblies");

        Directory.CreateDirectory(_assembliesDirectory);
    }

    /// <summary>
    /// Gets the default directory for storing compiled assemblies.
    /// </summary>
    public string AssembliesDirectory => _assembliesDirectory;

    /// <summary>
    /// Loads a function from its compiled assembly and returns the entry point method.
    /// </summary>
    public MethodInfo LoadFunction(Function function)
    {
        if (function.AssemblyPath == null || !File.Exists(function.AssemblyPath))
        {
            throw new InvalidOperationException(
                $"Function '{function.Name}' has not been compiled or assembly not found.");
        }

        // Unload if already loaded
        UnloadFunction(function.Id);

        var context = new FunctionLoadContext($"Function_{function.Id}", function.AssemblyPath);

        Assembly assembly;
        using (var fs = new FileStream(function.AssemblyPath, FileMode.Open, FileAccess.Read))
        {
            assembly = context.LoadFromStream(fs);
        }

        // Parse entry point: "Namespace.Class.Method"
        var entryPoint = function.EntryPoint;
        var lastDotIndex = entryPoint.LastIndexOf('.');
        if (lastDotIndex < 0)
        {
            throw new InvalidOperationException(
                $"Invalid entry point format: {entryPoint}. Expected 'Namespace.Class.Method'");
        }

        var typeName = entryPoint[..lastDotIndex];
        var methodName = entryPoint[(lastDotIndex + 1)..];

        var type = assembly.GetType(typeName)
            ?? throw new InvalidOperationException($"Type '{typeName}' not found in assembly.");

        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"Public static method '{methodName}' not found in type '{typeName}'.");

        _loadedFunctions[function.Id] = (context, method);
        return method;
    }

    /// <summary>
    /// Gets the loaded method for a function.
    /// </summary>
    public MethodInfo? GetLoadedMethod(Guid functionId)
    {
        return _loadedFunctions.TryGetValue(functionId, out var entry) ? entry.Method : null;
    }

    /// <summary>
    /// Invokes a loaded function with the given arguments.
    /// </summary>
    public async Task<object?> InvokeFunctionAsync(Guid functionId, params object?[] args)
    {
        if (!_loadedFunctions.TryGetValue(functionId, out var entry))
        {
            throw new InvalidOperationException($"Function {functionId} is not loaded.");
        }

        var result = entry.Method.Invoke(null, args);

        // Handle async methods
        if (result is Task task)
        {
            await task;

            var taskType = task.GetType();
            if (taskType.IsGenericType)
            {
                return taskType.GetProperty("Result")?.GetValue(task);
            }

            return null;
        }

        return result;
    }

    /// <summary>
    /// Unloads a function's assembly.
    /// </summary>
    public void UnloadFunction(Guid functionId)
    {
        if (_loadedFunctions.TryGetValue(functionId, out var entry))
        {
            _loadedFunctions.Remove(functionId);
            entry.Context.Unload();
        }
    }

    /// <summary>
    /// Unloads all function assemblies.
    /// </summary>
    public void UnloadAll()
    {
        foreach (var (_, (context, _)) in _loadedFunctions)
        {
            context.Unload();
        }
        _loadedFunctions.Clear();
    }

    /// <summary>
    /// Checks if a function is currently loaded.
    /// </summary>
    public bool IsLoaded(Guid functionId) => _loadedFunctions.ContainsKey(functionId);

    public void Dispose()
    {
        if (!_disposed)
        {
            UnloadAll();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
