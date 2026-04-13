
using RBALabs.Website.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<UmbracoBootstrapService>();

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();


await app.BootUmbracoAsync();

using (IServiceScope scope = app.Services.CreateScope())
{
    UmbracoBootstrapService bootstrapService = scope.ServiceProvider.GetRequiredService<UmbracoBootstrapService>();
    await bootstrapService.SeedAsync();
}


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
