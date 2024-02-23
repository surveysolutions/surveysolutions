using System;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests;

public class NumericTextFormatterTests : NumericTextFormatterTestsContext
{
    [Test]
    public void should_format_time()
    {
        Assert.That(NumericTextFormatter.FormatTimeHumanized(TimeSpan.FromSeconds(5)),
            Is.EqualTo("seconds"));
        
        Assert.That(NumericTextFormatter.FormatTimeHumanized(TimeSpan.FromDays(5)),
            Is.EqualTo("5 days"));
        
        Assert.That(NumericTextFormatter.FormatTimeHumanized(new TimeSpan(2,3,4,5)),
            Is.EqualTo("2 days 3 hours 4 minutes 5 seconds"));
        
        Assert.That(NumericTextFormatter.FormatTimeHumanized(new TimeSpan(2000,1,1,0)),
            Is.EqualTo("2000 days 1 hour 1 minute"));
        
        Assert.That(NumericTextFormatter.FormatTimeHumanized(new TimeSpan(2001,0,1,2)),
            Is.EqualTo("2001 days 1 minute 2 seconds"));
    }
    [Test]
    public void should_format_bytes()
    {
        Assert.That(NumericTextFormatter.FormatBytesHumanized(1023d),
            Is.EqualTo("1023 B"));
        Assert.That(NumericTextFormatter.FormatBytesHumanized(1023d * 1024),
            Is.EqualTo("1023 KB"));
        Assert.That(NumericTextFormatter.FormatBytesHumanized(1023d * 1024 * 1024),
            Is.EqualTo("1023 MB"));
        Assert.That(NumericTextFormatter.FormatBytesHumanized(1023d * 1024 * 1024 * 1024),
            Is.EqualTo("1023 GB"));
        Assert.That(NumericTextFormatter.FormatBytesHumanized(1023d * 1024 * 1024 * 1024 * 1024),
            Is.EqualTo("1023 TB"));
        Assert.That(NumericTextFormatter.FormatBytesHumanized(1023d * 1024 * 1024 * 1024 * 1024 * 1024),
            Is.EqualTo("1023 PB"));
    }
    [Test]
    public void should_format_speed()
    {
        Assert.That(NumericTextFormatter.FormatSpeedHumanized(1023d, TimeSpan.FromSeconds(1)),
            Is.EqualTo("1023 B/s"));
        Assert.That(NumericTextFormatter.FormatSpeedHumanized(1023d * 1024, TimeSpan.FromSeconds(1)),
            Is.EqualTo("1023 KB/s"));
        Assert.That(NumericTextFormatter.FormatSpeedHumanized(1023d * 1024 * 1024, TimeSpan.FromSeconds(1)),
            Is.EqualTo("1023 MB/s"));
        Assert.That(NumericTextFormatter.FormatSpeedHumanized(1023d * 1024 * 1024 * 1024, TimeSpan.FromSeconds(1)),
            Is.EqualTo("1023 GB/s"));
        Assert.That(NumericTextFormatter.FormatSpeedHumanized(1023d * 1024 * 1024 * 1024 * 1024, TimeSpan.FromSeconds(1)),
            Is.EqualTo("1023 TB/s"));
        Assert.That(NumericTextFormatter.FormatSpeedHumanized(1023d * 1024 * 1024 * 1024 * 1024 * 1024, TimeSpan.FromSeconds(1)),
            Is.EqualTo("1023 PB/s"));
    }
}
