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
                    TenantId UNIQUEIDENTIFIER NOT NULL,
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
                    TenantId UNIQUEIDENTIFIER NOT NULL,
                    PropertyId UNIQUEIDENTIFIER NOT NULL,
                    TenantPersonId NVARCHAR(100) NOT NULL,
                    StartDate DATETIME2 NOT NULL,
                    EndDate DATETIME2 NULL,
                    DueDayOfMonth INT NOT NULL,
                    IsActive BIT NOT NULL,
                    CONSTRAINT FK_Leases_Properties FOREIGN KEY (PropertyId) REFERENCES Properties(Id)
                );

                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Payments' AND xtype='U')
                CREATE TABLE Payments (
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    TenantId UNIQUEIDENTIFIER NOT NULL,
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
                    TenantId UNIQUEIDENTIFIER NOT NULL,
                    ResidentId NVARCHAR(100) NOT NULL,
                    PreviousScore INT NOT NULL,
                    NewScore INT NOT NULL,
                    Reason NVARCHAR(500) NULL,
                    CreatedAt DATETIME2 NOT NULL
                );

                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TrustScoreSnapshots' AND xtype='U')
                CREATE TABLE TrustScoreSnapshots (
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    TenantId UNIQUEIDENTIFIER NOT NULL,
                    UserId NVARCHAR(100) NOT NULL,
                    ScoreDate DATETIME2 NOT NULL,
                    ScoreValue INT NOT NULL,
                    Status INT NOT NULL
                );

                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TrustInsights' AND xtype='U')
                CREATE TABLE TrustInsights (
                    UserId NVARCHAR(100) NOT NULL,
                    TenantId UNIQUEIDENTIFIER NOT NULL,
                    CurrentScore INT NOT NULL,
                    TrendVector FLOAT NOT NULL,
                    MaturityLevel INT NOT NULL,
                    VolatilityFlag BIT NOT NULL,
                    AdjustedTrustLevel INT NOT NULL,
                    LastUpdated DATETIME2 NOT NULL,
                    PRIMARY KEY (UserId, TenantId)
                );

                    HistoricalWindow INT NOT NULL DEFAULT 6,
                    CreatedAt DATETIME2 NOT NULL
                );

                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OutboxMessages' AND xtype='U')
                CREATE TABLE OutboxMessages (
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    Type NVARCHAR(200) NOT NULL,
                    Content NVARCHAR(MAX) NOT NULL,
                    OccurredOnUtc DATETIME2 NOT NULL,
                    ProcessedOnUtc DATETIME2 NULL,
                    Error NVARCHAR(MAX) NULL
                );
            ";
            await connection.ExecuteAsync(sql);
        }
    }
}
