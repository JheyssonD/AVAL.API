using Microsoft.AspNetCore.Localization;
using System.Globalization;
using RentGuard.Presentation.API.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

var supportedCultures = new[] { "en-US", "es-ES", "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseMiddleware<RentGuard.Presentation.API.Infrastructure.Security.TenantMiddleware>();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

await DbInitializer.Initialize(app.Configuration);

app.Run();
