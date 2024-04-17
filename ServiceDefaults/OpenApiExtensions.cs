using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.Hosting;

public static class HostingExtensions
{
    public static WebApplication UseDefaultExceptionHandler(this WebApplication app, string? errorHandlingPath = null)
    {
        // The developer exception page is used automatically in development
        if (!app.Environment.IsDevelopment())
        {
            if (errorHandlingPath is not null)
            {
                app.UseExceptionHandler(errorHandlingPath);
            }
            else if (app.Services.GetService<IProblemDetailsService>() is not null)
            {
                // Default overload of UseExceptionHandler() requires ProblemDetails to be registered which is typically
                // only done in API apps so gate on that.
                app.UseExceptionHandler();
            }
        }

        return app;
    }
}

public static class OpenApiExtensions
{
    public static IApplicationBuilder UseDefaultOpenApi(this WebApplication app)
    {
        var configuration = app.Configuration;
        var openApiSection = configuration.GetSection("OpenApi");

        if (!openApiSection.Exists())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(setup =>
        {
            /// {
            ///   "OpenApi": {
            ///     "Endpoint: {
            ///         "Name": 
            ///     },
            ///     "Auth": {
            ///         "ClientId": ..,
            ///         "AppName": ..
            ///     }
            ///   }
            /// }

            var pathBase = configuration["PATH_BASE"];
            var authSection = openApiSection.GetSection("Auth");
            var endpointSection = openApiSection.GetRequiredSection("Endpoint");

            var swaggerUrl = endpointSection["Url"] ?? $"{pathBase}/swagger/v1/swagger.json";

            setup.SwaggerEndpoint(swaggerUrl, endpointSection.GetValue<string>("Name"));

            if (authSection.Exists())
            {
                setup.OAuthClientId(authSection.GetValue<string>("ClientId"));
                setup.OAuthAppName(authSection.GetValue<string>("AppName"));
            }
        });

        // Add a redirect from the root of the app to the swagger endpoint
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

        return app;
    }

    public static IHostApplicationBuilder AddDefaultOpenApi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        var openApi = configuration.GetSection("OpenApi");

        if (!openApi.Exists())
        {
            return builder;
        }

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen();

        // services.AddOptions<SwaggerGenOptions>()
        //     .Configure<ServiceEndPointResolverRegistry>((options, serviceEndPointResolver) =>
        //     {
        //         /// {
        //         ///   "OpenApi": {
        //         ///     "Document": {
        //         ///         "Title": ..
        //         ///         "Version": ..
        //         ///         "Description": ..
        //         ///     }
        //         ///   }
        //         /// }
        //         var document = openApi.GetRequiredSection("Document");
        //
        //         var version = document.GetValue<string>("Version") ?? "v1";
        //
        //         options.SwaggerDoc(version, new()
        //         {
        //             Title = document.GetValue<string>("Title"),
        //             Version = version,
        //             Description = document.GetValue<string>("Description")
        //         });
        //
        //         var identitySection = configuration.GetSection("Identity");
        //
        //         if (!identitySection.Exists())
        //         {
        //             // No identity section, so no authentication OpenAPI definition
        //             return;
        //         }
        //
        //         // {
        //         //   "Identity": {
        //         //     "Audience": "orders",
        //         //     "Scopes": {
        //         //         "basket": "Basket API"
        //         //      }
        //         //    }
        //         // }
        //
        //         var identityUri = serviceEndPointResolver.ResolveIdpAuthorityUri(configuration);
        //
        //         var scopes = identitySection.GetSection("Scopes").GetChildren().ToDictionary(p => p.Key, p => p.Value);
        //
        //         options.AddSecurityDefinition("oauth2", new()
        //         {
        //             Type = SecuritySchemeType.OAuth2,
        //             Flows = new OpenApiOAuthFlows
        //             {
        //                 // TODO: Change this to use Authorization Code flow with PKCE
        //                 Implicit = new()
        //                 {
        //                     AuthorizationUrl = new Uri(identityUri, "protocol/openid-connect/auth"),
        //                     TokenUrl = new Uri(identityUri, "protocol/openid-connect/token"),
        //                     Scopes = scopes,
        //                 }
        //             }
        //         });
        //
        //         options.OperationFilter<AuthorizeCheckOperationFilter>([scopes.Keys.ToArray()]);
        //     });

        return builder;
    }

    private sealed class AuthorizeCheckOperationFilter(string[] scopes) : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

            if (!metadata.OfType<IAuthorizeData>().Any())
            {
                return;
            }

            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

            var oAuthScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            };

            operation.Security =
            [
                new()
                {
                    [ oAuthScheme ] = scopes
                }
            ];
        }
    }
}