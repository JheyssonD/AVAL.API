using Dapper;
using Microsoft.Data.SqlClient;
using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class SqlPropertyRepository : IPropertyRepository
{
    private readonly string _connectionString;

    public SqlPropertyRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task AddAsync(Property property)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = "INSERT INTO Properties (Id, Title, Address, MonthlyRent, Currency, LandlordId, IsAvailable) VALUES (@Id, @Title, @Address, @MonthlyRent, @Currency, @LandlordId, @IsAvailable)";
        await connection.ExecuteAsync(sql, property);
    }

    public async Task<Property?> GetByIdAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Property>("SELECT * FROM Properties WHERE Id = @id", new { id });
    }
}

public class SqlLeaseRepository : ILeaseRepository
{
    private readonly string _connectionString;

    public SqlLeaseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task AddAsync(Lease lease)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = "INSERT INTO Leases (Id, PropertyId, TenantId, StartDate, EndDate, DueDayOfMonth, IsActive) VALUES (@Id, @PropertyId, @TenantId, @StartDate, @EndDate, @DueDayOfMonth, @IsActive)";
        await connection.ExecuteAsync(sql, lease);
    }
}
