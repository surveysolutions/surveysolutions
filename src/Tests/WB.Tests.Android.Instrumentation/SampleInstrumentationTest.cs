using NUnit.Framework;

namespace WB.Tests.Android.Instrumentation;

[TestFixture]
public class SampleInstrumentationTest
{
    [Test]
    public void when_running_on_device_should_pass()
    {
        Assert.That(true, Is.True);
    }
}
