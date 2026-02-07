using AgentDesigner.Domain.Models;
using Microsoft.Data.Sqlite;
using System.IO;

namespace AgentDesigner.Infrastructure.Persistence;

public class SqliteMetadataRepository
{
    private readonly string _connectionString;

    public SqliteMetadataRepository(string? dataDirectory = null)
    {
        var directory = dataDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AgentDesigner",
            "Projects");

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var dbPath = Path.Combine(directory, "metadata.db");
        _connectionString = $"Data Source={dbPath}";

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Create InputNodes table
        var createInputNodesCmd = connection.CreateCommand();
        createInputNodesCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS InputNodes (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Description TEXT
            );";
        createInputNodesCmd.ExecuteNonQuery();

        // Seed InputNodes if empty
        var checkInputCmd = connection.CreateCommand();
        checkInputCmd.CommandText = "SELECT COUNT(*) FROM InputNodes";
        var inputCount = (long?)checkInputCmd.ExecuteScalar() ?? 0;

        if (inputCount == 0)
        {
            using var transaction = connection.BeginTransaction();
            var insertCmd = connection.CreateCommand();
            insertCmd.Transaction = transaction;
            insertCmd.CommandText = @"
                INSERT INTO InputNodes (Id, Name, Description) VALUES
                (1, 'Console Input', 'An Input Node for collecting prompt from user on console'),
                (2, 'Scheduled Input', 'An input node triggered on a regular basis');";
            insertCmd.ExecuteNonQuery();
            transaction.Commit();
        }

        // Create Agents table
        var createAgentsCmd = connection.CreateCommand();
        createAgentsCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Agents (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Description TEXT
            );";
        createAgentsCmd.ExecuteNonQuery();

        // Seed Agents if empty
        var checkAgentsCmd = connection.CreateCommand();
        checkAgentsCmd.CommandText = "SELECT COUNT(*) FROM Agents";
        var agentsCount = (long?)checkAgentsCmd.ExecuteScalar() ?? 0;

        if (agentsCount == 0)
        {
            using var transaction = connection.BeginTransaction();
            var insertCmd = connection.CreateCommand();
            insertCmd.Transaction = transaction;
            insertCmd.CommandText = @"
                INSERT INTO Agents (Id, Name, Description) VALUES
                (1, 'Azure OpenAI Agent', 'Agent that is Azure OpenAI based');";
            insertCmd.ExecuteNonQuery();
            transaction.Commit();
        }
    }

    public List<MetadataItem> GetAllInputNodes()
    {
        var result = new List<MetadataItem>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name FROM InputNodes ORDER BY Name";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new MetadataItem(reader.GetInt32(0), reader.GetString(1)));
        }

        return result;
    }

    public List<MetadataItem> GetAllAgents()
    {
        var result = new List<MetadataItem>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name FROM Agents ORDER BY Name";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new MetadataItem(reader.GetInt32(0), reader.GetString(1)));
        }

        return result;
    }
}
