using JobDrop.Domain.Entities.Companies;
using JobDrop.Domain.Entities.Jobs;
using JobDrop.Domain.Entities.Messaging;
using JobDrop.Domain.Entities.ReverseDrops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobDrop.Infrastructure.Persistence.Configurations;

public sealed class CompanyCalendarConfig : IEntityTypeConfiguration<CompanyCalendar>
{
    public void Configure(EntityTypeBuilder<CompanyCalendar> b)
    {
        b.ToTable("CompanyCalendars");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.CompanyId).IsUnique();
    }
}

public sealed class CompanyHolidayConfig : IEntityTypeConfiguration<CompanyHoliday>
{
    public void Configure(EntityTypeBuilder<CompanyHoliday> b)
    {
        b.ToTable("CompanyHolidays");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.CompanyId, x.HolidayDate }).IsUnique();
    }
}

public sealed class JobAssignmentConfig : IEntityTypeConfiguration<JobAssignment>
{
    public void Configure(EntityTypeBuilder<JobAssignment> b)
    {
        b.ToTable("JobAssignments");
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.JobId, x.CompanyMemberId }).IsUnique();
    }
}

public sealed class ReverseDropPoolConfig : IEntityTypeConfiguration<ReverseDropPool>
{
    public void Configure(EntityTypeBuilder<ReverseDropPool> b)
    {
        b.ToTable("ReverseDropPools");
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => x.CompanyId);
    }
}

public sealed class ReverseDropConfig : IEntityTypeConfiguration<ReverseDrop>
{
    public void Configure(EntityTypeBuilder<ReverseDrop> b)
    {
        b.ToTable("ReverseDrops");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.IdempotencyKey).HasMaxLength(120).IsRequired();
        b.HasIndex(x => x.IdempotencyKey).IsUnique();
        b.HasIndex(x => new { x.CandidateProfileId, x.Status });
        b.HasIndex(x => x.ExpiresAtUtc);
    }
}

public sealed class ConversationConfig : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> b)
    {
        b.ToTable("Conversations");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.CandidateProfileId);
        b.HasIndex(x => x.CompanyId);
    }
}

public sealed class MessageConfig : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> b)
    {
        b.ToTable("Messages");
        b.HasKey(x => x.Id);
        b.Property(x => x.Body).HasMaxLength(4000);
        b.HasIndex(x => new { x.ConversationId, x.CreatedAtUtc });
    }
}

public sealed class InterviewProposalConfig : IEntityTypeConfiguration<InterviewProposal>
{
    public void Configure(EntityTypeBuilder<InterviewProposal> b)
    {
        b.ToTable("InterviewProposals");
        b.HasKey(x => x.Id);
        b.Property(x => x.CallType).HasMaxLength(20);
        b.Property(x => x.Status).HasMaxLength(20);
        b.HasIndex(x => new { x.ConversationId, x.Status });
    }
}