using System;
using FluentAssertions;
using JobDrop.Domain.ValueObjects;
using Xunit;

namespace JobDrop.Domain.UnitTests;

public sealed class GeoPointTests
{
    [Theory]
    [InlineData(55.2708, 25.2048, true)]
    [InlineData(-180, -90, true)]
    [InlineData(180, 90, true)]
    [InlineData(190, 25, false)]
    [InlineData(55, -95, false)]
    [InlineData(0, 0, false)]
    public void IsValid_ShouldValidateCoordinates(double lon, double lat, bool expected)
    {
        var point = new GeoPoint(lon, lat);
        var result = point.IsValid();
        result.Should().Be(expected);
    }
}