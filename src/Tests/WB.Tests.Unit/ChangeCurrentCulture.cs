using System;
using System.Globalization;
using System.Threading;

namespace WB.Tests.Unit
{
    public class ChangeCurrentCulture : IDisposable
    {
        private readonly CultureInfo original;

        public ChangeCurrentCulture(CultureInfo culture)
        {
            original = Thread.CurrentThread.CurrentCulture;
            Change(culture);
        }

        private void Change(CultureInfo culture)
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            Change(original);
        }
    }
}