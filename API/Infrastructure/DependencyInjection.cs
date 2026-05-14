using RentGuard.Core.Business.Modules.Payments.CreatePayment;
using RentGuard.Core.Business.Modules.Payments.ApprovePayment;
using RentGuard.Core.Business.Modules.Payments.GetPayments;
using RentGuard.Core.Business.Modules.TrustScore.GetTrustScore;
using RentGuard.Core.Business.Modules.AIValidation.ProcessValidation;
using RentGuard.Core.Business.Modules.Review.SubmitReview;
using RentGuard.Core.Business.Modules.Leases.CreateProperty;
using RentGuard.Core.Business.Modules.Leases.CreateLease;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;
using RentGuard.Core.Business.Modules.TrustScore.Domain.Repositories;
using RentGuard.Presentation.API.Infrastructure.Persistence;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddCoreHandlers();
        services.AddHostedService<RentGuard.Presentation.API.Infrastructure.BackgroundServices.OutboxProcessor>();
        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPropertyRepository, SqlPropertyRepository>();
        services.AddScoped<ILeaseRepository, SqlLeaseRepository>();
        services.AddScoped<IPaymentRepository, SqlPaymentRepository>();
        services.AddScoped<ITrustScoreRepository, SqlTrustScoreRepository>();
        services.AddScoped<IOutboxRepository, SqlOutboxRepository>();
        return services;
    }

    private static IServiceCollection AddCoreHandlers(this IServiceCollection services)
    {
        services.AddScoped<CreatePaymentHandler>();
        services.AddScoped<ApprovePaymentHandler>();
        services.AddScoped<GetPaymentsHandler>(); // NUEVO
        services.AddScoped<GetTrustScoreHandler>();
        services.AddScoped<ProcessValidationHandler>();
        services.AddScoped<SubmitReviewHandler>();
        services.AddScoped<CreatePropertyHandler>();
        services.AddScoped<CreateLeaseHandler>();
        return services;
    }
}
