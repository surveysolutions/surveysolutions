using System;
using System.IO;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Tests.Integration.InterviewTests;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Integration.WebTester.Services
{
    [TestOf(typeof(AppdomainsPerInterviewManager))]
    public class AppdomainsPerInterviewManagerTestsBase : InterviewTestsContext
    {
        protected AppdomainsPerInterviewManager CreateManager(IObservable<Guid> evictNotification = null)
        {
            var bin = Path.GetDirectoryName(typeof(when_configured).Assembly.Location);
            return new AppdomainsPerInterviewManager(bin, evictNotification, Mock.Of<ILogger>());
        }
    }
}