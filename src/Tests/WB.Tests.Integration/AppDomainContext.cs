using System;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Integration
{
    public class AppDomainContext : IDisposable
    {
        private AppDomainContext(AppDomain domain)
        {
            Domain = domain;
        }

        public static AppDomainContext Create()
        {
            var domain = AppDomain.CurrentDomain;
            return new AppDomainContext(domain);
        }

        public AppDomain Domain { get; }

        public void Dispose()
        {
        }
    }
}
