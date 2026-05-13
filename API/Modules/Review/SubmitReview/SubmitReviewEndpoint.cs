using FastEndpoints;
using RentGuard.Contracts.Modules.Review;
using RentGuard.Core.Business.Modules.Review.SubmitReview;

namespace RentGuard.API.Modules.Review.SubmitReview;

public class SubmitReviewEndpoint : Endpoint<SubmitReviewRequest>
{
    private readonly SubmitReviewHandler _handler;

    public SubmitReviewEndpoint(SubmitReviewHandler handler)
    {
        _handler = handler;
    }

    public override void Configure()
    {
        Post("/api/v1/reviews");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SubmitReviewRequest req, CancellationToken ct)
    {
        await _handler.Handle(req);
        await Send.NoContentAsync(ct);
    }
}
