using Microsoft.EntityFrameworkCore;
using RentGuard.Core.Business.Modules.Leases.Domain;
using RentGuard.Core.Business.Modules.Payments.Domain;
using RentGuard.Core.Business.Modules.TrustScore.Domain;
using RentGuard.Core.Business.Shared;
using RentGuard.Core.Business.Shared.Outbox;

namespace RentGuard.Presentation.API.Infrastructure.Persistence;

public class RentGuardDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public RentGuardDbContext(DbContextOptions<RentGuardDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<TrustScoreHistory> TrustScoreHistory => Set<TrustScoreHistory>();
    public DbSet<TrustScoreSnapshot> TrustScoreSnapshots => Set<TrustScoreSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Property Configuration
        modelBuilder.Entity<Property>(builder =>
        {
            builder.ToTable("Properties");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.MonthlyRent).HasPrecision(18, 2);
            builder.Property(x => x.RowVersion).IsRowVersion();
            builder.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        // Lease Configuration
        modelBuilder.Entity<Lease>(builder =>
        {
            builder.ToTable("Leases");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.MonthlyRent).HasPrecision(18, 2);
            builder.Property(x => x.CreditBalance).HasPrecision(18, 2);
            builder.Property(x => x.DebtBalance).HasPrecision(18, 2);
            builder.Property(x => x.RowVersion).IsRowVersion();
            builder.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(builder =>
        {
            builder.ToTable("Payments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.OCRConfidence).HasPrecision(5, 4);
            builder.Property(x => x.ExternalMessageId).HasMaxLength(255);
            builder.Property(x => x.ImageHash).HasMaxLength(128);
            builder.Property(x => x.RowVersion).IsRowVersion();
            builder.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        // Outbox Configuration
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(x => x.Id);
        });

        // Trust Score Configuration
        modelBuilder.Entity<TrustScoreHistory>(builder =>
        {
            builder.ToTable("TrustScoreHistory");
            builder.HasKey(x => x.Id);
            builder.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<TrustScoreSnapshot>(builder =>
        {
            builder.ToTable("TrustScoreSnapshots");
            builder.HasKey(x => x.Id);
            builder.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });
    }
}
