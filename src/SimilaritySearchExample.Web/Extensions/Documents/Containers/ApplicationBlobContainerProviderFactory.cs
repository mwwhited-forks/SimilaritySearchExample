using Eliassen.Documents.Containers;

namespace ResourceProfiler.Web.Extensions.Documents.Containers;

public class ApplicationBlobContainerProviderFactory : IBlobContainerProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ApplicationBlobContainerProviderFactory(
        IServiceProvider serviceProvider
        ) => _serviceProvider = serviceProvider;

    public IBlobContainerProvider Create(string containerName) =>
        ActivatorUtilities.CreateInstance<ApplicationBlobContainerProvider>(_serviceProvider, containerName);
}
