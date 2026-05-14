using Dapper;
using Microsoft.Data.SqlClient;
using RentGuard.Core.Business.Shared.Outbox;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class SqlOutboxRepository : IOutboxRepository
{
    private readonly string _connectionString;

    public SqlOutboxRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task SaveAsync(OutboxMessage message)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO OutboxMessages (Id, TenantId, Type, Content, OccurredOnUtc, ProcessedOnUtc, Error)
            VALUES (@Id, @TenantId, @Type, @Content, @OccurredOnUtc, @ProcessedOnUtc, @Error)";
        await connection.ExecuteAsync(sql, message);
    }

    public async Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            SELECT TOP (@BatchSize) Id, TenantId, Type, Content, OccurredOnUtc, ProcessedOnUtc, Error
            FROM OutboxMessages
            WHERE ProcessedOnUtc IS NULL
            ORDER BY OccurredOnUtc ASC";
        return await connection.QueryAsync<OutboxMessage>(sql, new { BatchSize = batchSize });
    }

    public async Task UpdateAsync(OutboxMessage message)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            UPDATE OutboxMessages
            SET ProcessedOnUtc = @ProcessedOnUtc, Error = @Error
            WHERE Id = @Id";
        await connection.ExecuteAsync(sql, message);
    }
}
