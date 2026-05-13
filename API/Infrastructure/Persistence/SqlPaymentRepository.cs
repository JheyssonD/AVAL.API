using Dapper;
using Microsoft.Data.SqlClient;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Modules.Payments.Domain.Repositories;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class SqlPaymentRepository : IPaymentRepository
{
    private readonly string _connectionString;
    public SqlPaymentRepository(IConfiguration configuration) => _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    public SqlPaymentRepository(string connectionString) => _connectionString = connectionString;

    public async Task AddAsync(Payment payment)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("INSERT INTO Payments (Id, LeaseId, Amount, PaymentDate, Reference, Status, CreatedAt) VALUES (@Id, @LeaseId, @Amount, @PaymentDate, @Reference, @Status, @CreatedAt)", payment);
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Payment>("SELECT * FROM Payments WHERE Id = @Id", new { Id = id });
    }

    public async Task UpdateAsync(Payment payment)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("UPDATE Payments SET Status = @Status WHERE Id = @Id", payment);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<Payment>("SELECT * FROM Payments ORDER BY CreatedAt DESC");
    }
}
