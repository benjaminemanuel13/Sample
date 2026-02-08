using Microsoft.Extensions.Logging;
using AgentDesigner.UI.ViewModels;
using AgentDesigner.UI.Views;
using AgentDesigner.UI.Services;
using AgentDesigner.Infrastructure.Persistence;
using AgentDesigner.Infrastructure.Assemblies;
using AgentDesigner.Infrastructure.Logging;
using AgentDesigner.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AgentDesigner.UI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Configure logging
        LoggingConfiguration.Configure();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register infrastructure services
        builder.Services.AddSingleton<JsonProjectRepository>();
        builder.Services.AddSingleton<FunctionAssemblyLoader>();
        builder.Services.AddSingleton<NavigationService>();
        builder.Services.AddSingleton<SqliteMetadataRepository>();
        builder.Services.AddSingleton<AgentDesigner.Infrastructure.Services.AssemblyGenerationService>();

        // Register application services
        builder.Services.AddSingleton<FunctionCompilationService>();
        builder.Services.AddSingleton<WorkflowExecutionService>();

        // Register ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<WorkflowDesignerViewModel>();
        builder.Services.AddTransient<ModelDesignerViewModel>();

        // Register Views
        builder.Services.AddTransient<MainView>();
        builder.Services.AddTransient<WorkflowDesignerView>();
        builder.Services.AddTransient<ModelDesignerView>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
