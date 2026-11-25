using JobDrop.Domain.Common;
using JobDrop.Domain.Enums;
namespace JobDrop.Domain.Entities.ReverseDrops;
public sealed class ReverseDrop : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid JobId { get; set; }
    public Guid SenderUserId { get; set; }
    public Guid CandidateProfileId { get; set; }
    public string? OfferDocumentKey { get; set; }
    public string? DispatchNote { get; set; }
    public bool IsSalaryNegotiable { get; set; }
    public ReverseDropStatus Status { get; set; } = ReverseDropStatus.Sent;
    public DateTime ExpiresAtUtc { get; set; }
    public string IdempotencyKey { get; set; } = default!;
    public byte[]? RowVersion { get; set; }
}