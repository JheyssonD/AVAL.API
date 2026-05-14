using Dapper;
using Microsoft.Data.SqlClient;
using RentGuard.Core.Business.Shared;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Leases.Domain.Repositories;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class SqlPropertyRepository : IPropertyRepository
{
    private readonly string _connectionString;
    private readonly ITenantContext _tenantContext;

    public SqlPropertyRepository(IConfiguration configuration, ITenantContext tenantContext)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(Property property)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO Properties (Id, TenantId, Title, Address, MonthlyRent, Currency, LandlordId, IsAvailable) 
            VALUES (@Id, @TenantId, @Title, @Address, @Rent, @Currency, @LandlordId, @IsAvailable)";
        
        await connection.ExecuteAsync(sql, new { 
            property.Id, 
            TenantId = _tenantContext.TenantId,
            property.Title, 
            property.Address, 
            Rent = property.MonthlyRent, 
            property.Currency, 
            property.LandlordId, 
            property.IsAvailable 
        });
    }

    public async Task<Property?> GetByIdAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT * FROM Properties WHERE Id = @Id AND TenantId = @TenantId";
        return await connection.QueryFirstOrDefaultAsync<Property>(sql, new { Id = id, TenantId = _tenantContext.TenantId });
    }

    public async Task<IEnumerable<Property>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT * FROM Properties WHERE TenantId = @TenantId";
        return await connection.QueryAsync<Property>(sql, new { TenantId = _tenantContext.TenantId });
    }
}

public class SqlLeaseRepository : ILeaseRepository
{
    private readonly string _connectionString;
    private readonly ITenantContext _tenantContext;

    public SqlLeaseRepository(IConfiguration configuration, ITenantContext tenantContext)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(Lease lease)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO Leases (Id, TenantId, PropertyId, TenantPersonId, StartDate, EndDate, DueDayOfMonth, IsActive) 
            VALUES (@Id, @SoftwareTenantId, @PropertyId, @TenantPersonId, @StartDate, @EndDate, @DueDayOfMonth, @IsActive)";
        
        await connection.ExecuteAsync(sql, new { 
            lease.Id,
            SoftwareTenantId = _tenantContext.TenantId,
            lease.PropertyId,
            lease.TenantId, // Note: The domain Lease entity likely has TenantId as the person
            StartDate = lease.StartDate,
            EndDate = lease.EndDate,
            DueDayOfMonth = lease.DueDayOfMonth,
            IsActive = lease.IsActive
        });
    }

    public async Task<Lease?> GetByIdAsync(Guid id)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT * FROM Leases WHERE Id = @Id AND TenantId = @TenantId";
        return await connection.QueryFirstOrDefaultAsync<Lease>(sql, new { Id = id, TenantId = _tenantContext.TenantId });
    }

    public async Task<IEnumerable<Lease>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = "SELECT * FROM Leases WHERE TenantId = @TenantId";
        return await connection.QueryAsync<Lease>(sql, new { TenantId = _tenantContext.TenantId });
    }
}

