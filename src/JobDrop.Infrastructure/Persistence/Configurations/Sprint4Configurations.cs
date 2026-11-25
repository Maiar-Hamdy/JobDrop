using JobDrop.Domain.Entities.Billing;
using JobDrop.Domain.Entities.Reviews;
using JobDrop.Domain.Entities.Notifications;
using JobDrop.Domain.Entities.AsyncOps;
using JobDrop.Domain.Entities.Candidate;
using JobDrop.Domain.Entities.Candidates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobDrop.Infrastructure.Persistence.Configurations;

public sealed class PlanConfig : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> b)
    {
        b.ToTable("Plans");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.Property(x => x.PriceAmount).HasPrecision(18, 2);
        b.HasIndex(x => new { x.Code, x.Version }).IsUnique();
    }
}

public sealed class SubscriptionConfig : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> b)
    {
        b.ToTable("Subscriptions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasMaxLength(20);
        b.HasIndex(x => new { x.UserId, x.Status });
    }
}

public sealed class PaymentConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("Payments");
        b.HasKey(x => x.Id);
        b.Property(x => x.MerchantReference).HasMaxLength(80).IsRequired();
        b.Property(x => x.ProviderOrderId).HasMaxLength(80).IsRequired();
        b.Property(x => x.AmountCents).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(3);
        b.HasIndex(x => x.MerchantReference).IsUnique();
        b.HasIndex(x => x.ProviderOrderId);
    }
}

public sealed class PaymentWebhookEventConfig : IEntityTypeConfiguration<PaymentWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookEvent> b)
    {
        b.ToTable("PaymentWebhookEvents");
        b.HasKey(x => x.Id);
        b.Property(x => x.ProviderTransactionId).HasMaxLength(80);
        b.Property(x => x.EventStateKey).HasMaxLength(20);
        b.Property(x => x.PayloadHash).HasMaxLength(64);
        b.HasIndex(x => new { x.ProviderTransactionId, x.EventStateKey }).IsUnique();
    }
}

public sealed class ReviewConfig : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> b)
    {
        b.ToTable("Reviews");
        b.HasKey(x => x.Id);
        b.Property(x => x.TargetType).HasMaxLength(20);
        b.Property(x => x.Status).HasMaxLength(20);
        b.HasIndex(x => new { x.ApplicationId, x.ReviewerUserId }).IsUnique();
        b.HasIndex(x => new { x.TargetType, x.TargetId, x.Status });
    }
}

public sealed class ReviewReportConfig : IEntityTypeConfiguration<ReviewReport>
{
    public void Configure(EntityTypeBuilder<ReviewReport> b)
    {
        b.ToTable("ReviewReports");
        b.HasKey(x => x.Id);
        b.Property(x => x.Reason).HasMaxLength(1000);
        b.HasIndex(x => new { x.ReviewId, x.Status });
    }
}

public sealed class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("OutboxMessages");
        b.HasKey(x => x.Id);
        b.Property(x => x.Channel).HasMaxLength(20);
        b.Property(x => x.EventType).HasMaxLength(80);
        b.HasIndex(x => new { x.DeliveredAtUtc, x.NextAttemptAtUtc });
        b.HasIndex(x => x.DeadLetteredAtUtc);
    }
}

public sealed class NotificationPreferenceConfig : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> b)
    {
        b.ToTable("NotificationPreferences");
        b.HasKey(x => x.Id);
        b.Property(x => x.Category).HasMaxLength(40);
        b.HasIndex(x => new { x.UserId, x.Category }).IsUnique();
    }
}

public sealed class ExportJobConfig : IEntityTypeConfiguration<ExportJob>
{
    public void Configure(EntityTypeBuilder<ExportJob> b)
    {
        b.ToTable("ExportJobs");
        b.HasKey(x => x.Id);
        b.Property(x => x.Kind).HasMaxLength(40);
        b.Property(x => x.Status).HasMaxLength(20);
        b.HasIndex(x => new { x.UserId, x.Status });
    }
}

public sealed class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.HasKey(x => x.Id);
        b.Property(x => x.ActorType).HasMaxLength(20);
        b.Property(x => x.Action).HasMaxLength(80);
        b.HasIndex(x => new { x.TargetType, x.TargetId, x.CreatedAtUtc });
    }
}

public sealed class CandidateDocumentConfig : IEntityTypeConfiguration<CandidateDocument>
{
    public void Configure(EntityTypeBuilder<CandidateDocument> b)
    {
        b.ToTable("CandidateDocuments");
        b.HasKey(x => x.Id);
        b.Property(x => x.StorageKey).HasMaxLength(300).IsRequired();
        b.Property(x => x.Sha256Hash).HasMaxLength(64);
        b.HasIndex(x => new { x.CandidateProfileId, x.ScanStatus });
    }
}