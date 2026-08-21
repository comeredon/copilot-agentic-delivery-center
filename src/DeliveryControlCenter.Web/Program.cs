using DeliveryControlCenter.Web.Components;
using DeliveryControlCenter.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<DeliveryApiClient>(client =>
{
    // Service discovery resolves this to the API resource address at runtime.
    client.BaseAddress = new Uri("https+http://api");
});

builder.Services.AddScoped<DeliveryRealtimeClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseOutputCache();

app.MapStaticAssets();
app.MapDefaultEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
