using Microsoft.AspNetCore.Mvc;
using RentGuard.Presentation.Contracts.Modules.Leases;
using RentGuard.Core.Business.Modules.Leases.CreateProperty;
using RentGuard.Core.Business.Modules.Leases.CreateLease;

namespace RentGuard.Presentation.API.Controllers;

[ApiController]
[Route("api/v1/leases")]
public class LeasesController : ControllerBase
{
    private readonly CreatePropertyHandler _propertyHandler;
    private readonly CreateLeaseHandler _leaseHandler;

    public LeasesController(CreatePropertyHandler propertyHandler, CreateLeaseHandler leaseHandler)
    {
        _propertyHandler = propertyHandler;
        _leaseHandler = leaseHandler;
    }

    [HttpPost("properties")]
    public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyRequest request)
    {
        var command = new CreatePropertyCommand(request.Title, request.Address, request.Rent, request.Currency, request.LandlordId);
        await _propertyHandler.Handle(command, CancellationToken.None);
        return Ok(new { Message = "Property created successfully" });
    }

    [HttpPost("leases")]
    public async Task<IActionResult> CreateLease([FromBody] CreateLeaseRequest request)
    {
        var command = new CreateLeaseCommand(request.PropertyId, request.TenantId, request.StartDate, request.DueDayOfMonth);
        await _leaseHandler.Handle(command, CancellationToken.None);
        return Ok(new { Message = "Lease created successfully" });
    }
}
