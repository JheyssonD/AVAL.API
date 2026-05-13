using Dapper;
using Microsoft.Data.SqlClient;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task Initialize(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var masterConnectionString = connectionString.Replace("Database=RentGuardDb", "Database=master");
        
        using (var masterConnection = new SqlConnection(masterConnectionString))
        {
            await masterConnection.ExecuteAsync("IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RentGuardDb') CREATE DATABASE RentGuardDb;");
        }

        await Task.Delay(1000);

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

                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
                CREATE TABLE Payments (
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    LeaseId UNIQUEIDENTIFIER NOT NULL,
                    Amount DECIMAL(18,2) NOT NULL,
                    PaymentDate DATETIME2 NOT NULL,
                    Reference NVARCHAR(100) NULL,
                    Status INT NOT NULL,
                    CreatedAt DATETIME2 NOT NULL,
                    CONSTRAINT FK_Payments_Leases FOREIGN KEY (LeaseId) REFERENCES Leases(Id)
                );

                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TrustScoreHistory' AND xtype='U')
                CREATE TABLE TrustScoreHistory (
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    TenantId NVARCHAR(100) NOT NULL,
                    PreviousScore INT NOT NULL,
                    NewScore INT NOT NULL,
                    Reason NVARCHAR(500) NULL,
                    CreatedAt DATETIME2 NOT NULL
                );
            ";
            await connection.ExecuteAsync(sql);
        }
    }
}
