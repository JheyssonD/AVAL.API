using RentGuard.Presentation.API.Infrastructure.Persistence;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Inyeccin de Infraestructura
        builder.Services.AddInfrastructure(builder.Configuration);
        
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Inicializacin de DB Blindada
        await DbInitializer.Initialize(app.Configuration);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        
        app.MapControllers();

        await app.RunAsync();
    }
}
