using System.Text.Json;
using System.Text.Json.Serialization;
using AgentDesigner.Domain.Entities;
using AgentDesigner.Domain.Interfaces;

namespace AgentDesigner.Infrastructure.Persistence;

/// <summary>
/// JSON file-based implementation of IProjectRepository.
/// </summary>
public class JsonProjectRepository : IProjectRepository
{
    private readonly string _projectsDirectory;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Dictionary<Guid, Project> _cache = [];

    public JsonProjectRepository(string? projectsDirectory = null)
    {
        _projectsDirectory = projectsDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AgentDesigner",
            "Projects");

        Directory.CreateDirectory(_projectsDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public async Task<Project> CreateAsync(Project project)
    {
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        _cache[project.Id] = project;

        if (project.FilePath != null)
        {
            await SaveToFileAsync(project, project.FilePath);
        }

        return project;
    }

    public Task<Project?> GetByIdAsync(Guid id)
    {
        _cache.TryGetValue(id, out var project);
        return Task.FromResult(project);
    }

    public Task<IEnumerable<Project>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Project>>(_cache.Values);
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        _cache[project.Id] = project;

        if (project.FilePath != null)
        {
            await SaveToFileAsync(project, project.FilePath);
        }

        return project;
    }

    public Task DeleteAsync(Guid id)
    {
        if (_cache.TryGetValue(id, out var project))
        {
            _cache.Remove(id);

            if (project.FilePath != null && File.Exists(project.FilePath))
            {
                File.Delete(project.FilePath);
            }
        }

        return Task.CompletedTask;
    }

    public async Task SaveToFileAsync(Project project, string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        project.FilePath = filePath;
        project.UpdatedAt = DateTime.UtcNow;

        var json = JsonSerializer.Serialize(project, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<Project> LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Project file not found: {filePath}");
        }

        var json = await File.ReadAllTextAsync(filePath);
        var project = JsonSerializer.Deserialize<Project>(json, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize project");

        project.FilePath = filePath;
        _cache[project.Id] = project;

        return project;
    }
}
