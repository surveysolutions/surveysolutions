using System;
using System.Globalization;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;

namespace WB.Tests.Unit.BoundedContexts.Headquarters
{
    public class ExtensionsTest
    {
        [Test]
        public void Should_format_date_properly()
        {
            // November 22, 1963 at 12:30 p.m.
            var date = new DateTime(1963, 11, 22, 12, 30, 00);
            using (new ChangeCurrentCulture(CultureInfo.GetCultureInfo("en-US")))
            {
                var formatted = date.FormatDateWithTime();
                Assert.That(formatted, Is.EqualTo("Nov 22, 1963 12:30"));
            }
        }
    }
}
