using JobDrop.Application.Contracts.Persistence;
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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ApplicationEntity = JobDrop.Domain.Entities.Drops.Application;

namespace JobDrop.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AppIdentityUser, AppIdentityRole, Guid>(options), IApplicationDbContext
{
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<StartingPoint> StartingPoints => Set<StartingPoint>();
    public DbSet<SavedRegion> SavedRegions => Set<SavedRegion>();
    public DbSet<SavedJob> SavedJobs => Set<SavedJob>();
    public DbSet<PinnedDrop> PinnedDrops => Set<PinnedDrop>();
    public DbSet<CandidateDocument> CandidateDocuments => Set<CandidateDocument>();
    public DbSet<DropWalletBalance> DropWalletBalances => Set<DropWalletBalance>();
    public DbSet<DropLedgerEntry> DropLedgerEntries => Set<DropLedgerEntry>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyOffice> CompanyOffices => Set<CompanyOffice>();
    public DbSet<CompanyMember> CompanyMembers => Set<CompanyMember>();
    public DbSet<CompanyCalendar> CompanyCalendars => Set<CompanyCalendar>();
    public DbSet<CompanyHoliday> CompanyHolidays => Set<CompanyHoliday>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobDailyIntake> JobDailyIntakes => Set<JobDailyIntake>();
    public DbSet<JobAssignment> JobAssignments => Set<JobAssignment>();
    public DbSet<ApplicationEntity> Applications => Set<ApplicationEntity>();
    public DbSet<ApplicationSnapshot> ApplicationSnapshots => Set<ApplicationSnapshot>();
    public DbSet<ApplicationSla> ApplicationSlas => Set<ApplicationSla>();
    public DbSet<ApplicationEvent> ApplicationEvents => Set<ApplicationEvent>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
    public DbSet<OperatingArea> OperatingAreas => Set<OperatingArea>();
    public DbSet<ReverseDropPool> ReverseDropPools => Set<ReverseDropPool>();
    public DbSet<ReverseDrop> ReverseDrops => Set<ReverseDrop>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<InterviewProposal> InterviewProposals => Set<InterviewProposal>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentWebhookEvent> PaymentWebhookEvents => Set<PaymentWebhookEvent>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewReport> ReviewReports => Set<ReviewReport>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<ExportJob> ExportJobs => Set<ExportJob>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public Task<int> GetUserCountAsync(CancellationToken ct)
        => Users.CountAsync(ct);

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}