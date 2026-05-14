using Dapper;
using Microsoft.Data.SqlClient;
using RentGuard.Core.Business.Shared;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class SqlPaymentRepository : IPaymentRepository
{
    private readonly string _connectionString;
    private readonly ITenantContext _tenantContext;

    public SqlPaymentRepository(IConfiguration configuration, ITenantContext tenantContext)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(Payment payment)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO Payments (Id, TenantId, LeaseId, Amount, PaymentDate, Reference, Status, CreatedAt) 
            VALUES (@Id, @SoftwareTenantId, @LeaseId, @Amount, @PaymentDate, @Reference, @Status, @CreatedAt)";
        
        await connection.ExecuteAsync(sql, new { 
            payment.Id,
            SoftwareTenantId = _tenantContext.TenantId,
            payment.LeaseId,
            payment.Amount,
            payment.PaymentDate,
            payment.Reference,
            payment.Status,
            payment.CreatedAt
        });
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT * FROM Payments WHERE Id = @Id AND TenantId = @TenantId";
        return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new { Id = id, TenantId = _tenantContext.TenantId });
    }

    public async Task UpdateAsync(Payment payment)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = "UPDATE Payments SET Status = @Status WHERE Id = @Id AND TenantId = @TenantId";
        await connection.ExecuteAsync(sql, new { payment.Status, payment.Id, TenantId = _tenantContext.TenantId });
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT * FROM Payments WHERE TenantId = @TenantId ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<Payment>(sql, new { TenantId = _tenantContext.TenantId });
    }
}

