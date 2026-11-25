using JobDrop.Domain.Entities.Candidates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobDrop.Infrastructure.Persistence.Configurations;

public sealed class SavedRegionConfig : IEntityTypeConfiguration<SavedRegion>
{
    public void Configure(EntityTypeBuilder<SavedRegion> b)
    {
        b.ToTable("SavedRegions");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.CandidateProfileId, x.OperatingAreaId }).IsUnique();
    }
}

public sealed class SavedJobConfig : IEntityTypeConfiguration<SavedJob>
{
    public void Configure(EntityTypeBuilder<SavedJob> b)
    {
        b.ToTable("SavedJobs");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.CandidateProfileId, x.JobId }).IsUnique();
    }
}

public sealed class PinnedDropConfig : IEntityTypeConfiguration<PinnedDrop>
{
    public void Configure(EntityTypeBuilder<PinnedDrop> b)
    {
        b.ToTable("PinnedDrops");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.CandidateProfileId, x.ApplicationId }).IsUnique();
        b.ToTable(t => t.HasCheckConstraint("CK_PinnedDrops_PinOrder", "[PinOrder] BETWEEN 1 AND 3"));
    }
}