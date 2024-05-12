using eShop.WebApp.Components;
using eShop.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// builder.AddServiceDefaults();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// builder.Services.AddHttpForwarderWithServiceDiscovery();

// Application services
builder.Services.AddSingleton<IProductImageUrlProvider, ProductImageUrlProvider>();

// HTTP and gRPC client registrations
builder.Services.AddHttpClient<CatalogService>(o => o.BaseAddress = new("http://localhost:51234"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

// app.MapDefaultEndpoints();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// app.MapForwarder("/product-images/{id}", "http://catalog-api", "/api/v1/catalog/items/{id}/pic");

app.Run();
