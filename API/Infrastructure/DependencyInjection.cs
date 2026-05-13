using RentGuard.Core.Business.Modules.Payments.CreatePayment;
using RentGuard.Core.Business.Modules.TrustScore.GetTrustScore;
using RentGuard.Core.Business.Modules.AIValidation.ProcessValidation;
using RentGuard.Core.Business.Modules.Review.SubmitReview;
using RentGuard.Core.Business.Modules.Leases.CreateProperty;
using RentGuard.Core.Business.Modules.Leases.CreateLease;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using RentGuard.Presentation.API.Infrastructure.Persistence;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddCoreHandlers();
        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPropertyRepository, SqlPropertyRepository>();
        services.AddScoped<ILeaseRepository, SqlLeaseRepository>();
        return services;
    }

    private static IServiceCollection AddCoreHandlers(this IServiceCollection services)
    {
        services.AddScoped<CreatePaymentHandler>();
        services.AddScoped<GetTrustScoreHandler>();
        services.AddScoped<ProcessValidationHandler>();
        services.AddScoped<SubmitReviewHandler>();
        services.AddScoped<CreatePropertyHandler>();
        services.AddScoped<CreateLeaseHandler>();
        return services;
    }
}
