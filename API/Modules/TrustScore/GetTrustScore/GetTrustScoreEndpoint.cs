using FastEndpoints;
using RentGuard.Contracts.Modules.TrustScore;
using RentGuard.Core.Business.Modules.TrustScore.GetTrustScore;

namespace RentGuard.API.Modules.TrustScore.GetTrustScore;

public class GetTrustScoreEndpoint : Endpoint<GetTrustScoreRequest, GetTrustScoreResponse>
{
    private readonly GetTrustScoreHandler _handler;

    public GetTrustScoreEndpoint(GetTrustScoreHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Get("/api/v1/trust-score/{TenantId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetTrustScoreRequest req, CancellationToken ct)
    {
        var response = await _handler.Handle(req);
        await Send.OkAsync(response, ct);
    }
}
