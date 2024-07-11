using NUnit.Framework;

namespace WB.Tests.Unit.Designer;

public static class NUnitExtensions
{
    public static void NotNull<TActual>(this Assert assert, TActual value)
    {
        Assert.That(value, Is.Not.Null);
    }
}
