using Dapper;
using Microsoft.Data.SqlClient;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Presentation.API.Infrastructure.Persistence;
using Xunit;
using FluentAssertions;

namespace RentGuard.Core.Business.Tests;

public class SqlRepositoryIntegrationTests
{
    private readonly string _connectionString = "Server=localhost;Database=RentGuardDb;User Id=sa;Password=RentGuard_Strong_Pass_2026!;TrustServerCertificate=True;";
    
    public SqlRepositoryIntegrationTests()
    {
        var masterConnStr = _connectionString.Replace("Database=RentGuardDb", "Database=master");
        using var masterConn = new SqlConnection(masterConnStr);
        masterConn.Execute("IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RentGuardDb') CREATE DATABASE RentGuardDb;");
        
        using var conn = new SqlConnection(_connectionString);
        conn.Execute(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
            CREATE TABLE Payments (
                Id UNIQUEIDENTIFIER PRIMARY KEY,
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
        var repo = new SqlPaymentRepository(_connectionString);
        var payment = Payment.Create(Guid.NewGuid(), 1500.50m, DateTime.UtcNow, "SQL-REF-OK");

        await repo.AddAsync(payment);
        var retrieved = await repo.GetByIdAsync(payment.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Reference.Should().Be("SQL-REF-OK");
    }
}
