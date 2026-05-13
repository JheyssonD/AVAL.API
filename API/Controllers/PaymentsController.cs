using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RentGuard.Core.Business;
using RentGuard.Core.Business.Modules.Payments.CreatePayment;
using RentGuard.Core.Business.Modules.Payments.ApprovePayment;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;

namespace RentGuard.Presentation.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly CreatePaymentHandler _createPaymentHandler;
    private readonly ApprovePaymentHandler _approvePaymentHandler;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public PaymentsController(
        CreatePaymentHandler createPaymentHandler,
        ApprovePaymentHandler approvePaymentHandler,
        IPaymentRepository paymentRepository,
        IStringLocalizer<SharedResources> localizer)
    {
        _createPaymentHandler = createPaymentHandler;
        _approvePaymentHandler = approvePaymentHandler;
        _paymentRepository = paymentRepository;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var payments = await _paymentRepository.GetAllAsync();
        return Ok(payments);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentCommand command)
    {
        await _createPaymentHandler.Handle(command, HttpContext.RequestAborted);
        return Ok(new { Message = _localizer[DomainErrors.Payments.Created.Code].Value });
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        await _approvePaymentHandler.Handle(new ApprovePaymentCommand(id), HttpContext.RequestAborted);
        return Ok(new { Message = _localizer[DomainErrors.Payments.Approved.Code].Value });
    }
}
