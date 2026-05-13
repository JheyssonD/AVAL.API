using FastEndpoints;
using RentGuard.Contracts.Modules.AIValidation;
using RentGuard.Core.Business.Modules.AIValidation.ProcessValidation;

namespace RentGuard.API.Modules.AIValidation.ProcessValidation;

public class ValidateReceiptEndpoint : Endpoint<ExtractedData, ValidationResultResponse>
{
    private readonly ProcessValidationHandler _handler;

    public ValidateReceiptEndpoint(ProcessValidationHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/api/v1/ai/validate");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ExtractedData req, CancellationToken ct)
    {
        var response = await _handler.Handle(req, 1000m);
        await Send.OkAsync(response, ct);
    }
}
