using JobDrop.Domain.Entities.Candidates;
using JobDrop.Domain.Entities.Companies;
using JobDrop.Domain.Entities.Drops;
using JobDrop.Domain.Entities.Identity;
using JobDrop.Domain.Entities.Jobs;
using JobDrop.Domain.Entities.Reference;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApplicationEntity = JobDrop.Domain.Entities.Drops.Application;

namespace JobDrop.Infrastructure.Persistence.Configurations;

public sealed class CandidateProfileConfig : IEntityTypeConfiguration<CandidateProfile>
{
    public void Configure(EntityTypeBuilder<CandidateProfile> b)
    {
        b.ToTable("CandidateProfiles");
        b.HasKey(x => x.Id);
        b.Property(x => x.Summary).HasMaxLength(4000);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => x.UserId).IsUnique();
    }
}

public sealed class StartingPointConfig : IEntityTypeConfiguration<StartingPoint>
{
    public void Configure(EntityTypeBuilder<StartingPoint> b)
    {
        b.ToTable("StartingPoints");
        b.HasKey(x => x.Id);
        b.Property(x => x.Location).HasColumnType("geography").IsRequired();
        b.Property(x => x.Label).HasMaxLength(150);
        b.HasIndex(x => new { x.CandidateProfileId, x.IsActiveOrigin });
    }
}

public sealed class DropWalletConfig : IEntityTypeConfiguration<DropWalletBalance>
{
    public void Configure(EntityTypeBuilder<DropWalletBalance> b)
    {
        b.ToTable("DropWalletBalances");
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => x.CandidateProfileId).IsUnique();
    }
}

public sealed class DropLedgerConfig : IEntityTypeConfiguration<DropLedgerEntry>
{
    public void Configure(EntityTypeBuilder<DropLedgerEntry> b)
    {
        b.ToTable("DropLedgerEntries");
        b.HasKey(x => x.Id);
        b.Property(x => x.Bucket).HasMaxLength(20);
        b.Property(x => x.ReasonCode).HasMaxLength(60);
        b.Property(x => x.IdempotencyKey).HasMaxLength(120).IsRequired();
        b.HasIndex(x => x.IdempotencyKey).IsUnique();
        b.HasIndex(x => new { x.CandidateProfileId, x.CreatedAtUtc });
    }
}

public sealed class CompanyConfig : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> b)
    {
        b.ToTable("Companies");
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => x.VerificationLevel);
    }
}

public sealed class CompanyOfficeConfig : IEntityTypeConfiguration<CompanyOffice>
{
    public void Configure(EntityTypeBuilder<CompanyOffice> b)
    {
        b.ToTable("CompanyOffices");
        b.HasKey(x => x.Id);
        b.Property(x => x.Location).HasColumnType("geography").IsRequired();
        b.Property(x => x.Nickname).HasMaxLength(150).IsRequired();
        b.Property(x => x.OfficeType).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.LocationVersion).IsConcurrencyToken();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<OperatingArea>().WithMany().HasForeignKey(x => x.OperatingAreaId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.CompanyId, x.VerificationLevel });
    }
}

public sealed class CompanyMemberConfig : IEntityTypeConfiguration<CompanyMember>
{
    public void Configure(EntityTypeBuilder<CompanyMember> b)
    {
        b.ToTable("CompanyMembers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(x => new { x.CompanyId, x.UserId }).IsUnique();
    }
}

public sealed class JobConfig : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> b)
    {
        b.ToTable("Jobs");
        b.HasKey(x => x.Id);
        b.Property(x => x.SalaryMin).HasPrecision(18, 2);
        b.Property(x => x.SalaryMax).HasPrecision(18, 2);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.WorkMode).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => new { x.CompanyId, x.Status });
        b.HasIndex(x => new { x.Status, x.PublishedAtUtc });
    }
}

public sealed class JobDailyIntakeConfig : IEntityTypeConfiguration<JobDailyIntake>
{
    public void Configure(EntityTypeBuilder<JobDailyIntake> b)
    {
        b.ToTable("JobDailyIntakes");
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => new { x.JobId, x.MarketDate }).IsUnique();
    }
}

public sealed class ApplicationConfig : IEntityTypeConfiguration<ApplicationEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationEntity> b)
    {
        b.ToTable("Applications");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => new { x.CandidateProfileId, x.JobId }).IsUnique();
        b.HasIndex(x => new { x.JobId, x.Status, x.DeliveredAtUtc });
    }
}

public sealed class ApplicationSnapshotConfig : IEntityTypeConfiguration<ApplicationSnapshot>
{
    public void Configure(EntityTypeBuilder<ApplicationSnapshot> b)
    {
        b.ToTable("ApplicationSnapshots");
        b.HasKey(x => x.Id);
        b.Property(x => x.MatchScore).HasPrecision(5, 2);
        b.HasIndex(x => x.ApplicationId).IsUnique();
    }
}

public sealed class ApplicationSlaConfig : IEntityTypeConfiguration<ApplicationSla>
{
    public void Configure(EntityTypeBuilder<ApplicationSla> b)
    {
        b.ToTable("ApplicationSlas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => x.ApplicationId).IsUnique();
        b.HasIndex(x => new { x.Status, x.DueAtUtc });
    }
}

public sealed class ApplicationEventConfig : IEntityTypeConfiguration<ApplicationEvent>
{
    public void Configure(EntityTypeBuilder<ApplicationEvent> b)
    {
        b.ToTable("ApplicationEvents");
        b.HasKey(x => x.Id);
        b.Property(x => x.EventType).HasMaxLength(40);
        b.HasIndex(x => new { x.ApplicationId, x.OccurredAtUtc });
    }
}

public sealed class UserSessionConfig : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> b)
    {
        b.ToTable("UserSessions");
        b.HasKey(x => x.Id);
        b.Property(x => x.RefreshTokenHash).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.RefreshTokenHash).IsUnique();
        b.HasIndex(x => new { x.UserId, x.RevokedAtUtc });
    }
}

public sealed class CatalogItemConfig : IEntityTypeConfiguration<CatalogItem>
{
    public void Configure(EntityTypeBuilder<CatalogItem> b)
    {
        b.ToTable("CatalogItems");
        b.HasKey(x => x.Id);
        b.Property(x => x.Category).HasMaxLength(30);
        b.Property(x => x.Code).HasMaxLength(60);
        b.HasIndex(x => new { x.Category, x.Code }).IsUnique();
    }
}

public sealed class OperatingAreaConfig : IEntityTypeConfiguration<OperatingArea>
{
    public void Configure(EntityTypeBuilder<OperatingArea> b)
    {
        b.ToTable("OperatingAreas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Boundary).HasColumnType("geography");
        b.Property(x => x.CenterPoint).HasColumnType("geography");
        b.HasIndex(x => x.MarketCode).IsUnique();
    }
}