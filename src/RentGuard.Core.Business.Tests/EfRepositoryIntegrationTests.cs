using Microsoft.EntityFrameworkCore;
using Moq;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Shared;
using RentGuard.Presentation.API.Infrastructure.Persistence;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class EfRepositoryIntegrationTests
{
    private readonly RentGuardDbContext _context;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Guid _tenantId = Guid.NewGuid();

    public EfRepositoryIntegrationTests()
    {
        _tenantContextMock = new Mock<ITenantContext>();
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);

        var options = new DbContextOptionsBuilder<RentGuardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RentGuardDbContext(options, _tenantContextMock.Object);
    }

    [Fact]
    public async Task Should_Persist_And_Retrieve_Payment_With_Tenant_Isolation()
    {
        // Arrange
        var repo = new EfPaymentRepository(_context);
        var leaseId = Guid.NewGuid();
        var payment = Payment.Create(leaseId, 1500.50m, DateTime.UtcNow, "EF-REF-OK");
        
        // Simular el TenantId asignado por la lógica de negocio o infraestructura
        typeof(Payment).GetProperty("TenantId")!.SetValue(payment, _tenantId);

        // Act
        await repo.AddAsync(payment);
        
        // Reset context to verify persistence
        _context.ChangeTracker.Clear();
        
        var retrieved = await repo.GetByIdAsync(payment.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Reference.Should().Be("EF-REF-OK");
        retrieved.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task Should_Filter_By_TenantId_Automatically()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var leaseId = Guid.NewGuid();
        
        var p1 = Payment.Create(leaseId, 100, DateTime.UtcNow, "P1");
        typeof(Payment).GetProperty("TenantId")!.SetValue(p1, _tenantId);
        
        var p2 = Payment.Create(leaseId, 200, DateTime.UtcNow, "P2");
        typeof(Payment).GetProperty("TenantId")!.SetValue(p2, otherTenantId);

        _context.Payments.AddRange(p1, p2);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var repo = new EfPaymentRepository(_context);
        var results = await repo.GetAllAsync();

        // Assert
        results.Should().HaveCount(1);
        results.First().Reference.Should().Be("P1");
    }
}

