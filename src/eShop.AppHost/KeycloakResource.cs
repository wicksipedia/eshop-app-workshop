namespace Aspire.Hosting;

internal static class KeycloakHostingExtensions
{
    private const int DefaultContainerPort = 8080;

    public static IResourceBuilder<KeycloakContainerResource> AddKeycloakContainer(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null,
        string? tag = null,
        string? configPath = null)
    {
        var keycloakContainer = new KeycloakContainerResource(name);

        var tempBuilder = builder
            .AddResource(keycloakContainer)
            .WithAnnotation(new ContainerImageAnnotation
                { Registry = "quay.io", Image = "keycloak/keycloak", Tag = tag ?? "latest" })
            .WithHttpEndpoint(port: port, targetPort: DefaultContainerPort)
            .WithEnvironment("KEYCLOAK_ADMIN", "admin")
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
            .WithArgs("start-dev");
        // .WithManifestPublishingCallback(context => WriteKeycloakContainerToManifest(context, keycloakContainer));

        if (!string.IsNullOrWhiteSpace(configPath))
        {
            tempBuilder
                .WithBindMount(configPath, "/opt/keycloak/data/import")
                .WithArgs("--import-realm");
        }

        return tempBuilder;
    }
}

internal class KeycloakContainerResource(string name) : 
    ContainerResource(name),
    IResourceWithServiceDiscovery
{
}