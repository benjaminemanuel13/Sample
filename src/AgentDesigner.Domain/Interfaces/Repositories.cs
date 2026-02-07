using AgentDesigner.Domain.Entities;

namespace AgentDesigner.Domain.Interfaces;

/// <summary>
/// Repository interface for project persistence.
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Creates a new project.
    /// </summary>
    Task<Project> CreateAsync(Project project);

    /// <summary>
    /// Gets a project by ID.
    /// </summary>
    Task<Project?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all projects.
    /// </summary>
    Task<IEnumerable<Project>> GetAllAsync();

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    Task<Project> UpdateAsync(Project project);

    /// <summary>
    /// Deletes a project.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Saves project to a file.
    /// </summary>
    Task SaveToFileAsync(Project project, string filePath);

    /// <summary>
    /// Loads project from a file.
    /// </summary>
    Task<Project> LoadFromFileAsync(string filePath);
}

/// <summary>
/// Repository interface for agent persistence.
/// </summary>
public interface IAgentRepository
{
    Task<Agent> CreateAsync(Agent agent);
    Task<Agent?> GetByIdAsync(Guid id);
    Task<IEnumerable<Agent>> GetAllAsync();
    Task<Agent> UpdateAsync(Agent agent);
    Task DeleteAsync(Guid id);
}

/// <summary>
/// Repository interface for function persistence.
/// </summary>
public interface IFunctionRepository
{
    Task<Function> CreateAsync(Function function);
    Task<Function?> GetByIdAsync(Guid id);
    Task<IEnumerable<Function>> GetAllAsync();
    Task<Function> UpdateAsync(Function function);
    Task DeleteAsync(Guid id);
}

/// <summary>
/// Repository interface for workflow persistence.
/// </summary>
public interface IWorkflowRepository
{
    Task<Workflow> CreateAsync(Workflow workflow);
    Task<Workflow?> GetByIdAsync(Guid id);
    Task<IEnumerable<Workflow>> GetAllAsync();
    Task<Workflow> UpdateAsync(Workflow workflow);
    Task DeleteAsync(Guid id);
}
