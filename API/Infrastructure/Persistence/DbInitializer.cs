using Dapper;
using Microsoft.Data.SqlClient;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task Initialize(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        
        // 1. Conectar a 'master' para asegurar que la DB existe
        var masterConnectionString = connectionString.Replace("Database=RentGuardDb", "Database=master")
                                                   .Replace("Initial Catalog=RentGuardDb", "Initial Catalog=master");
        
        using (var masterConnection = new SqlConnection(masterConnectionString))
        {
            await masterConnection.ExecuteAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RentGuardDb')
                CREATE DATABASE RentGuardDb;
            ");
        }

        // Dar un pequeo respiro al SQL Server
        await Task.Delay(2000);

        // 2. Ahora s, conectar a 'RentGuardDb' e inicializar tablas
        using (var connection = new SqlConnection(connectionString))
        {
            const string sql = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Properties' AND xtype='U')
                CREATE TABLE Properties (
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    Title NVARCHAR(200) NOT NULL,
                    Address NVARCHAR(500) NOT NULL,
                    MonthlyRent DECIMAL(18,2) NOT NULL,
                    Currency NVARCHAR(10) NOT NULL,
                    LandlordId NVARCHAR(100) NOT NULL,
                    IsAvailable BIT NOT NULL
                );

                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Leases' AND xtype='U')
                CREATE TABLE Leases (
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    PropertyId UNIQUEIDENTIFIER NOT NULL,
                    TenantId NVARCHAR(100) NOT NULL,
                    StartDate DATETIME2 NOT NULL,
                    EndDate DATETIME2 NULL,
                    DueDayOfMonth INT NOT NULL,
                    IsActive BIT NOT NULL,
                    CONSTRAINT FK_Leases_Properties FOREIGN KEY (PropertyId) REFERENCES Properties(Id)
                );
            ";
            await connection.ExecuteAsync(sql);
        }
    }
}
