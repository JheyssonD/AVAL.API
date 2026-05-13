using FastEndpoints;
using RentGuard.Core.Business.Modules.Payments.CreatePayment;
using RentGuard.Core.Business.Modules.TrustScore.GetTrustScore;
using RentGuard.Core.Business.Modules.AIValidation.ProcessValidation;
using RentGuard.Core.Business.Modules.Review.SubmitReview;



public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddFastEndpoints();
        builder.Services.AddOpenApi();

        // Inyección de Handlers del Core
        builder.Services.AddScoped<CreatePaymentHandler>();
        builder.Services.AddScoped<GetTrustScoreHandler>();
        builder.Services.AddScoped<ProcessValidationHandler>();
        builder.Services.AddScoped<SubmitReviewHandler>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseFastEndpoints();

        app.Run();
    }
}
