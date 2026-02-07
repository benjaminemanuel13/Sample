using Serilog;
using Serilog.Events;

namespace AgentDesigner.Infrastructure.Logging;

/// <summary>
/// Configures and provides application-wide logging.
/// </summary>
public static class LoggingConfiguration
{
    private static bool _isConfigured;

    /// <summary>
    /// Configures Serilog logging for the application.
    /// </summary>
    public static void Configure(string? logDirectory = null)
    {
        if (_isConfigured) return;

        var logPath = logDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AgentDesigner",
            "Logs");

        Directory.CreateDirectory(logPath);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: Path.Combine(logPath, "agentdesigner-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        _isConfigured = true;
    }

    /// <summary>
    /// Gets the configured logger.
    /// </summary>
    public static ILogger Logger => Log.Logger;

    /// <summary>
    /// Closes and flushes the logger.
    /// </summary>
    public static void CloseAndFlush() => Log.CloseAndFlush();
}
