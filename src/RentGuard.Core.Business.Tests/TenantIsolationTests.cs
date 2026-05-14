using Moq;
using RentGuard.Core.Business.Shared;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class TenantIsolationTests
{
    private readonly Mock<ITenantContext> _tenantContextMock;

    public TenantIsolationTests()
    {
        _tenantContextMock = new Mock<ITenantContext>();
    }

    [Fact]
    public void TenantContext_ShouldAllowSettingId()
    {
        var context = new TenantContext();
        var tenantId = Guid.NewGuid();
        
        context.SetTenantId(tenantId);
        
        context.TenantId.Should().Be(tenantId);
    }

    // Nota: Las pruebas de repositorio requieren una DB real o un wrapper.
    // Aquí definimos el comportamiento esperado para la lógica de aislamiento.
}
