using Dapper;
using Microsoft.Data.SqlClient;
using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class SqlPropertyRepository : IPropertyRepository
{
    private readonly string _connectionString;
    public SqlPropertyRepository(IConfiguration configuration) => _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    public async Task AddAsync(Property property)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("INSERT INTO Properties (Id, Title, Address, MonthlyRent, Currency, LandlordId, IsAvailable) VALUES (@Id, @Title, @Address, @Rent, @Currency, @LandlordId, @IsAvailable)", 
            new { property.Id, property.Title, property.Address, Rent = property.MonthlyRent, property.Currency, property.LandlordId, property.IsAvailable });
    }
    public async Task<Property?> GetByIdAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Property>("SELECT * FROM Properties WHERE Id = @Id", new { Id = id });
    }
    public async Task<IEnumerable<Property>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<Property>("SELECT * FROM Properties");
    }
}

public class SqlLeaseRepository : ILeaseRepository
{
    private readonly string _connectionString;
    public SqlLeaseRepository(IConfiguration configuration) => _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    public async Task AddAsync(Lease lease)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("INSERT INTO Leases (Id, PropertyId, TenantId, StartDate, EndDate, DueDayOfMonth, IsActive) VALUES (@Id, @PropertyId, @TenantId, @StartDate, @EndDate, @DueDayOfMonth, @IsActive)", lease);
    }
    public async Task<Lease?> GetByIdAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Lease>("SELECT * FROM Leases WHERE Id = @Id", new { Id = id });
    }
    public async Task<IEnumerable<Lease>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<Lease>("SELECT * FROM Leases");
    }
}
