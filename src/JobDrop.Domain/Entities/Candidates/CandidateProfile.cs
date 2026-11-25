using JobDrop.Domain.Common;
namespace JobDrop.Domain.Entities.Candidates;
public sealed class CandidateProfile : AggregateRoot
{
    public Guid UserId { get; set; }
    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public string? PreferredWorkModes { get; set; }
    public string SharingProtocolJson { get; set; } = "{}";
    public string? DropThemeCode { get; set; }
    public byte[]? RowVersion { get; set; }
}