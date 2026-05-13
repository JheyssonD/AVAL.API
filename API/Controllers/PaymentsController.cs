using Microsoft.AspNetCore.Mvc;
using RentGuard.Core.Business.Modules.Payments.CreatePayment;
using RentGuard.Core.Business.Modules.Payments.ApprovePayment;
using RentGuard.Core.Business.Modules.Payments.GetPayments;

namespace RentGuard.Presentation.API.Controllers;

[ApiController]
[Route("api/v1/payments")]
public class PaymentsController : ControllerBase
{
    private readonly CreatePaymentHandler _createHandler;
    private readonly ApprovePaymentHandler _approveHandler;
    private readonly GetPaymentsHandler _getHandler;

    public PaymentsController(
        CreatePaymentHandler createHandler, 
        ApprovePaymentHandler approveHandler,
        GetPaymentsHandler getHandler)
    {
        _createHandler = createHandler;
        _approveHandler = approveHandler;
        _getHandler = getHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetPayments()
    {
        var payments = await _getHandler.Handle(CancellationToken.None);
        return Ok(payments);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        var command = new CreatePaymentCommand(request.LeaseId, request.Amount, DateTime.UtcNow, request.Reference);
        await _createHandler.Handle(command, CancellationToken.None);
        return Ok(new { Message = "Payment registered successfully" });
    }

    [HttpPost("approve")]
    public async Task<IActionResult> ApprovePayment([FromBody] ApprovePaymentRequest request)
    {
        var command = new ApprovePaymentCommand(request.PaymentId);
        await _approveHandler.Handle(command, CancellationToken.None);
        return Ok(new { Message = "Payment approved and TrustScore updated" });
    }
}

public record CreatePaymentRequest(Guid LeaseId, decimal Amount, string Reference);
public record ApprovePaymentRequest(Guid PaymentId);
