using RentGuard.Core.Business.Shared;

namespace RentGuard.Presentation.API.Infrastructure.Security;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
        {
            if (Guid.TryParse(tenantIdHeader, out var tenantId))
            {
                tenantContext.SetTenantId(tenantId);
            }
        }

        await _next(context);
    }
}
