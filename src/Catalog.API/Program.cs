using eShop.Catalog.API;
using eShop.Catalog.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<CatalogDbContext>("CatalogDB");

builder.Services.Configure<CatalogOptions>(builder.Configuration.GetSection(nameof(CatalogOptions)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapDefaultEndpoints();

app.MapGroup(app.GetOptions<CatalogOptions>().ApiBasePath)
    .WithTags("Catalog API")
    .MapCatalogApi();

app.Run();
