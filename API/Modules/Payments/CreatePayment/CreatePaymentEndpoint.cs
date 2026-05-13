using FastEndpoints;
using RentGuard.Contracts.Modules.Payments;
using RentGuard.Core.Business.Modules.Payments.CreatePayment;

namespace RentGuard.API.Modules.Payments.CreatePayment;

public class CreatePaymentEndpoint : Endpoint<CreatePaymentRequest, CreatePaymentResponse>
{
    private readonly CreatePaymentHandler _handler;

    public CreatePaymentEndpoint(CreatePaymentHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/api/v1/payments");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreatePaymentRequest req, CancellationToken ct)
    {
        var response = await _handler.Handle(req);
        await Send.OkAsync(response, ct);
    }
}
