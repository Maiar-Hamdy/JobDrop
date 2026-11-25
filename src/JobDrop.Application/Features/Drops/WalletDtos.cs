namespace JobDrop.Application.Features.Drops;
public sealed record WalletDto(int DailyRemaining, int DailyLimit, DateTime DailyResetAtUtc, int BonusBalance, int SpecialBalance);
public sealed record LedgerDto(Guid Id, string Bucket, int Delta, string ReasonCode, Guid? ReferenceId, DateTime CreatedAtUtc);