using JobDrop.Domain.Entities.Candidates;
using JobDrop.Domain.Entities.Companies;
using JobDrop.Domain.Entities.Drops;
using JobDrop.Domain.Entities.Identity;
using JobDrop.Domain.Entities.Jobs;
using JobDrop.Domain.Entities.Reference;
using JobDrop.Domain.Entities.ReverseDrops;
using JobDrop.Domain.Entities.Messaging;
using JobDrop.Domain.Entities.Billing;
using JobDrop.Domain.Entities.Reviews;
using JobDrop.Domain.Entities.Notifications;
using JobDrop.Domain.Entities.AsyncOps;
using JobDrop.Domain.Entities.Candidate;
using Microsoft.EntityFrameworkCore;
using ApplicationEntity = JobDrop.Domain.Entities.Drops.Application;

namespace JobDrop.Application.Contracts.Persistence;

public interface IApplicationDbContext
{
    DbSet<CandidateProfile> CandidateProfiles { get; }
    DbSet<StartingPoint> StartingPoints { get; }
    DbSet<SavedRegion> SavedRegions { get; }
    DbSet<SavedJob> SavedJobs { get; }
    DbSet<PinnedDrop> PinnedDrops { get; }
    DbSet<CandidateDocument> CandidateDocuments { get; }
    DbSet<DropWalletBalance> DropWalletBalances { get; }
    DbSet<DropLedgerEntry> DropLedgerEntries { get; }
    DbSet<Company> Companies { get; }
    DbSet<CompanyOffice> CompanyOffices { get; }
    DbSet<CompanyMember> CompanyMembers { get; }
    DbSet<CompanyCalendar> CompanyCalendars { get; }
    DbSet<CompanyHoliday> CompanyHolidays { get; }
    DbSet<Job> Jobs { get; }
    DbSet<JobDailyIntake> JobDailyIntakes { get; }
    DbSet<JobAssignment> JobAssignments { get; }
    DbSet<ApplicationEntity> Applications { get; }
    DbSet<ApplicationSnapshot> ApplicationSnapshots { get; }
    DbSet<ApplicationSla> ApplicationSlas { get; }
    DbSet<ApplicationEvent> ApplicationEvents { get; }
    DbSet<UserSession> UserSessions { get; }
    DbSet<CatalogItem> CatalogItems { get; }
    DbSet<OperatingArea> OperatingAreas { get; }
    DbSet<ReverseDropPool> ReverseDropPools { get; }
    DbSet<ReverseDrop> ReverseDrops { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }
    DbSet<InterviewProposal> InterviewProposals { get; }
    DbSet<Plan> Plans { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentWebhookEvent> PaymentWebhookEvents { get; }
    DbSet<Review> Reviews { get; }
    DbSet<ReviewReport> ReviewReports { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }
    DbSet<ExportJob> ExportJobs { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> GetUserCountAsync(CancellationToken ct);
}