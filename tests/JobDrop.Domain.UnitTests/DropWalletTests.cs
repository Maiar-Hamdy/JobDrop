using System;
using FluentAssertions;
using JobDrop.Domain.Entities.Candidates;
using JobDrop.Domain.Enums;
using Xunit;

namespace JobDrop.Domain.UnitTests;

public sealed class DropWalletTests
{
    [Fact]
    public void TryDebitOne_ShouldDebitFromDailyFirst()
    {
        var now = new DateTime(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);
        var wallet = new DropWalletBalance
        {
            DailyLimit = 3,
            DailyRemaining = 2,
            DailyResetAtUtc = now.AddDays(1),
            BonusBalance = 5,
            SpecialBalance = 1
        };
        var bucket = wallet.TryDebitOne(now);
        bucket.Should().Be(DropBucket.Daily);
        wallet.DailyRemaining.Should().Be(1);
        wallet.BonusBalance.Should().Be(5);
        wallet.SpecialBalance.Should().Be(1);
    }

    [Fact]
    public void TryDebitOne_ShouldDebitFromBonus_WhenDailyExhausted()
    {
        var now = new DateTime(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);
        var wallet = new DropWalletBalance
        {
            DailyLimit = 3,
            DailyRemaining = 0,
            DailyResetAtUtc = now.AddDays(1),
            BonusBalance = 2,
            SpecialBalance = 1
        };
        var bucket = wallet.TryDebitOne(now);
        bucket.Should().Be(DropBucket.Bonus);
        wallet.DailyRemaining.Should().Be(0);
        wallet.BonusBalance.Should().Be(1);
    }

    [Fact]
    public void TryDebitOne_ShouldResetDaily_WhenResetTimePassed()
    {
        var now = new DateTime(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);
        var wallet = new DropWalletBalance
        {
            DailyLimit = 3,
            DailyRemaining = 0,
            DailyResetAtUtc = now.AddHours(-1),
            BonusBalance = 0,
            SpecialBalance = 0
        };
        var bucket = wallet.TryDebitOne(now);
        bucket.Should().Be(DropBucket.Daily);
        wallet.DailyRemaining.Should().Be(2);
        wallet.DailyResetAtUtc.Should().Be(now.Date.AddDays(1));
    }

    [Fact]
    public void TryDebitOne_ShouldReturnNull_WhenAllBucketsEmpty()
    {
        var now = new DateTime(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);
        var wallet = new DropWalletBalance
        {
            DailyLimit = 3,
            DailyRemaining = 0,
            DailyResetAtUtc = now.AddDays(1),
            BonusBalance = 0,
            SpecialBalance = 0
        };
        var bucket = wallet.TryDebitOne(now);
        bucket.Should().BeNull();
    }
}