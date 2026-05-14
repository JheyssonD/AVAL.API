using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using Moq;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Shared;
using RentGuard.Presentation.API.Infrastructure.Persistence;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class EfRepositoryIntegrationTests : IDisposable
{
    private readonly RentGuardDbContext _context;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Guid _tenantId = Guid.NewGuid();

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    public EfRepositoryIntegrationTests()
    {
        _tenantContextMock = new Mock<ITenantContext>();
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var baseConnStr = config.GetConnectionString("TestConnection");
        var dbName = $"RentGuardDb_Test_{Guid.NewGuid():N}";
        var connectionString = baseConnStr!.Replace("RentGuardDb_Test", dbName);

        var options = new DbContextOptionsBuilder<RentGuardDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        _context = new RentGuardDbContext(options, _tenantContextMock.Object);
        _context.Database.EnsureCreated();
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

    [Fact]
    public async Task Update_Should_Throw_ConcurrencyException_On_Conflict()
    {
        // Arrange
        var repo = new EfPaymentRepository(_context);
        var leaseId = Guid.NewGuid();
        var payment = Payment.Create(leaseId, 100, DateTime.UtcNow, "P1");
        typeof(Payment).GetProperty("TenantId")!.SetValue(payment, _tenantId);
        
        await repo.AddAsync(payment);
        _context.ChangeTracker.Clear();

        // Obtener dos instancias del mismo pago
        var p1 = await repo.GetByIdAsync(payment.Id);
        var p2 = await _context.Payments.AsNoTracking().FirstAsync(p => p.Id == payment.Id);

        // Modificar p1 y guardar
        p1!.Approve();
        await repo.UpdateAsync(p1);

        // Intentar modificar p2 (que tiene el RowVersion viejo)
        p2.Reject();
        
        // Assert
        Func<Task> act = async () => {
            _context.Payments.Update(p2);
            await _context.SaveChangesAsync();
        };

        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}

