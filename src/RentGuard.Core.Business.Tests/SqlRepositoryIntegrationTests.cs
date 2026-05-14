using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Moq;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Shared;
using RentGuard.Presentation.API.Infrastructure.Persistence;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class SqlRepositoryIntegrationTests
{
    private readonly string _connectionString = "Server=localhost;Database=RentGuardDb;User Id=sa;Password=RentGuard_Strong_Pass_2026!;TrustServerCertificate=True;";
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Guid _tenantId = Guid.NewGuid();

    public SqlRepositoryIntegrationTests()
    {
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"]).Returns(_connectionString);
        
        _tenantContextMock = new Mock<ITenantContext>();
        _tenantContextMock.Setup(t => t.TenantId).Returns(_tenantId);

        var masterConnStr = _connectionString.Replace("Database=RentGuardDb", "Database=master");
        using var masterConn = new SqlConnection(masterConnStr);
        masterConn.Execute("IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RentGuardDb') CREATE DATABASE RentGuardDb;");
        
        using var conn = new SqlConnection(_connectionString);
        conn.Execute(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
            CREATE TABLE Payments (
                Id UNIQUEIDENTIFIER PRIMARY KEY,
                TenantId UNIQUEIDENTIFIER NOT NULL,
                LeaseId UNIQUEIDENTIFIER NOT NULL,
                Amount DECIMAL(18,2) NOT NULL,
                PaymentDate DATETIME2 NOT NULL,
                Reference NVARCHAR(100) NULL,
                Status INT NOT NULL,
                CreatedAt DATETIME2 NOT NULL
            );
        ");
    }

    [Fact]
    public async Task Should_Persist_And_Retrieve_Payment()
    {
        var repo = new SqlPaymentRepository(_configMock.Object, _tenantContextMock.Object);
        
        // Insertar dependencias para cumplir con FKs
        var propertyId = Guid.NewGuid();
        var leaseId = Guid.NewGuid();
        
        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.ExecuteAsync("INSERT INTO Properties (Id, TenantId, Title, Address, MonthlyRent, Currency, LandlordId, IsAvailable) VALUES (@Id, @Tid, 'Test Prop', 'Addr', 1000, 'USD', 'L1', 1)", new { Id = propertyId, Tid = _tenantId });
            await conn.ExecuteAsync("INSERT INTO Leases (Id, TenantId, PropertyId, TenantPersonId, StartDate, DueDayOfMonth, IsActive) VALUES (@Id, @Tid, @PropId, 'T1', GETUTCDATE(), 5, 1)", new { Id = leaseId, Tid = _tenantId, PropId = propertyId });
        }

        var payment = Payment.Create(leaseId, 1500.50m, DateTime.UtcNow, "SQL-REF-OK");

        await repo.AddAsync(payment);
        var retrieved = await repo.GetByIdAsync(payment.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Reference.Should().Be("SQL-REF-OK");
    }
}

