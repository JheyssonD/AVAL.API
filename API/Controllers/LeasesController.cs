using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RentGuard.Core.Business;
using RentGuard.Core.Business.Modules.Leases.CreateLease;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;

namespace RentGuard.Presentation.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LeasesController : ControllerBase
{
    private readonly CreateLeaseHandler _createLeaseHandler;
    private readonly ILeaseRepository _leaseRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public LeasesController(
        CreateLeaseHandler createLeaseHandler, 
        ILeaseRepository leaseRepository,
        IPropertyRepository propertyRepository,
        IStringLocalizer<SharedResources> localizer)
    {
        _createLeaseHandler = createLeaseHandler;
        _leaseRepository = leaseRepository;
        _propertyRepository = propertyRepository;
        _localizer = localizer;
    }

    [HttpPost("properties")]
    public async Task<IActionResult> CreateProperty([FromBody] RentGuard.Core.Business.Modules.Leases.Domain.Property property)
    {
        await _propertyRepository.AddAsync(property);
        return Ok(new { Message = _localizer[DomainErrors.Properties.Created.Code].Value });
    }

    [HttpPost]
    public async Task<IActionResult> CreateLease([FromBody] CreateLeaseCommand command)
    {
        await _createLeaseHandler.Handle(command, HttpContext.RequestAborted);
        return Ok(new { Message = _localizer[DomainErrors.Leases.Created.Code].Value });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var leases = await _leaseRepository.GetAllAsync();
        return Ok(leases);
    }
}
