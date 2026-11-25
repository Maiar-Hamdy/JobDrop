using JobDrop.Domain.Common;
using JobDrop.Domain.Enums;
namespace JobDrop.Domain.Entities.Candidates;

public sealed class DropWalletBalance : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public int DailyRemaining { get; set; }
    public int DailyLimit { get; set; }
    public DateTime DailyResetAtUtc { get; set; }
    public int BonusBalance { get; set; }
    public int SpecialBalance { get; set; }
    public byte[]? RowVersion { get; set; }

    /// <summary>Debit 1 credit in precedence Daily → Bonus → Special. Pure logic (no DB).</summary>
    public DropBucket? TryDebitOne(DateTime utcNow)
    {
        if (utcNow >= DailyResetAtUtc) { DailyRemaining = DailyLimit; DailyResetAtUtc = utcNow.Date.AddDays(1); }
        if (DailyRemaining > 0) { DailyRemaining--; return DropBucket.Daily; }
        if (BonusBalance > 0) { BonusBalance--; return DropBucket.Bonus; }
        if (SpecialBalance > 0) { SpecialBalance--; return DropBucket.Special; }
        return null;
    }

    public void CreditBonus(int amount) => BonusBalance += amount;
    public void CreditSpecial(int amount) => SpecialBalance += amount;
}