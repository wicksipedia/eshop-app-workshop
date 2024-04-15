var builder = DistributedApplication.CreateBuilder(args);

// Databases

var basketStore = builder.AddRedis("BasketStore")
    .WithDataVolume()
    .WithRedisCommander();

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var catalogDb = postgres.AddDatabase("CatalogDB");

// Identity Providers

var idp = builder.AddKeycloakContainer("idp", tag: "23.0", configPath: "../Keycloak/data/import");

// DB Manager Apps

builder.AddProject<Projects.Catalog_Data_Manager>("catalog-db-mgr")
    .WithReference(catalogDb);

// API Apps

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb);

var basketApi = builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(basketStore)
    .WithReference(idp);


// Apps

// Force HTTPS profile for web app (required for OIDC operations)
var webApp = builder.AddProject<Projects.WebApp>("webapp", launchProfileName: "https")
    .WithReference(catalogApi)
    .WithReference(basketApi)
    .WithReference(idp);

// Inject the project URLs for Keycloak realm configuration
idp.WithEnvironment("WEBAPP_HTTP", () => webApp.GetEndpoint("http").Url);
idp.WithEnvironment("WEBAPP_HTTPS", () => webApp.GetEndpoint("https").Url);

// Inject assigned URLs for Catalog API
catalogApi.WithEnvironment("CatalogOptions__PicBaseAddress", () => catalogApi.GetEndpoint("http").Url);

builder.Build().Run();