using AgenTerra.Core.Knowledge;
using AgenTerra.Core.Reasoning;
using AgenTerra.Core.State;
using Microsoft.Extensions.DependencyInjection;

namespace AgenTerra.Core;

/// <summary>
/// Extension methods for configuring AgenTerra services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AgenTerra core services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// Note: ReasoningTool and InMemoryWorkflowStateStore implement IDisposable and are registered as singletons.
    /// They will be automatically disposed when the DI container is disposed (typically on application shutdown).
    /// Ensure the application's service provider is properly disposed to release these resources.
    /// </remarks>
    public static IServiceCollection AddAgenTerra(this IServiceCollection services)
    {
        // Reasoning - Singleton for shared session management
        services.AddSingleton<IReasoningTool, ReasoningTool>();
        
        // State - Singleton for shared state storage
        services.AddSingleton<IWorkflowStateStore, InMemoryWorkflowStateStore>();
        
        // Knowledge - Singleton for shared factory with registered readers
        // The factory creates and manages its own document reader instances
        services.AddSingleton<DocumentReaderFactory>();
        
        return services;
    }
}
