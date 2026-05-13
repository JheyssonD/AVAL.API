using FastEndpoints;
using RentGuard.Presentation.API.Infrastructure.Persistence;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddInfrastructure(builder.Configuration);
        
        builder.Services.AddControllers(); // Asegurar soporte de controladores
        builder.Services.AddFastEndpoints();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        await DbInitializer.Initialize(app.Configuration);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        
        // Endpoints de prueba directos
        app.MapGet("/health", () => "API is alive");
        
        app.MapControllers();
        app.UseFastEndpoints();

        await app.RunAsync();
    }
}
