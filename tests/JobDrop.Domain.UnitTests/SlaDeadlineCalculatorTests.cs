using System;
using FluentAssertions;
using JobDrop.Domain.Services;
using Xunit;

namespace JobDrop.Domain.UnitTests;

public sealed class SlaDeadlineCalculatorTests
{
    [Fact]
    public void ComputeDueAtUtc_ShouldAdd48Hours_ForStrictPolicy()
    {
        var deliveredAt = new DateTime(2026, 11, 25, 10, 0, 0, DateTimeKind.Utc);
        var dueAt = SlaDeadlineCalculator.ComputeDueAtUtc(deliveredAt, "Strict48hElapsed");
        dueAt.Should().Be(new DateTime(2026, 11, 27, 10, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ComputeDueAtUtc_ShouldDefaultTo48Hours_ForUnknownPolicy()
    {
        var deliveredAt = new DateTime(2026, 11, 25, 10, 0, 0, DateTimeKind.Utc);
        var dueAt = SlaDeadlineCalculator.ComputeDueAtUtc(deliveredAt, "UnknownPolicy");
        dueAt.Should().Be(new DateTime(2026, 11, 27, 10, 0, 0, DateTimeKind.Utc));
    }
}