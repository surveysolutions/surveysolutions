using System;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Tester.Infrastructure.Internals.Log
{
    public class XamarinInsightsLoggerProvider : ILoggerProvider
    {
        public ILogger GetFor<T>()
        {
            return new XamarinInsightsLogger();
        }

        public ILogger GetForType(Type type)
        {
            return new XamarinInsightsLogger();
        }
    }
}