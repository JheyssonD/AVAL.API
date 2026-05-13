using Dapper;
using Microsoft.Data.SqlClient;
using RentGuard.Core.Business.Modules.TrustScore.Domain;
using RentGuard.Core.Business.Modules.TrustScore.Domain.Repositories;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class SqlTrustScoreRepository : ITrustScoreRepository
{
    private readonly string _connectionString;

    public SqlTrustScoreRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<int> GetCurrentScoreAsync(string tenantId)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT TOP 1 NewScore FROM TrustScoreHistory WHERE TenantId = @TenantId ORDER BY CreatedAt DESC";
        var lastScore = await connection.QueryFirstOrDefaultAsync<int?>(sql, new { TenantId = tenantId });
        return lastScore ?? 80; // Puntaje inicial por defecto
    }

    public async Task UpdateScoreAsync(string tenantId, int newScore)
    {
        // El score se actualiza de forma efectiva mediante la insercin en el historial (Audit-Only)
        await Task.CompletedTask; 
    }

    public async Task AddHistoryAsync(TrustScoreHistory history)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO TrustScoreHistory (Id, TenantId, PreviousScore, NewScore, Reason, CreatedAt)
            VALUES (@Id, @TenantId, @PreviousScore, @NewScore, @Reason, @CreatedAt)";
        await connection.ExecuteAsync(sql, history);
    }
}
