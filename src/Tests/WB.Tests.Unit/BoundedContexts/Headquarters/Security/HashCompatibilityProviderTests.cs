using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Users.Providers;
using WB.Core.Infrastructure.Versions;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Security
{
    public class HashCompatibilityProviderTests
    {
        [TestCase("3.0.0", ExpectedResult = true)]
        [TestCase("6.0.0", ExpectedResult = false)]
        [TestCase(null, ExpectedResult = false)]
        public bool IsInSha1CompatibilityModeTest(string currentVersion)
        {
            var versionHistory =
                Mock.Of<IProductVersionHistory>(
                    pvh => pvh.GetHistory() == new[] {new ProductVersionChange(currentVersion, DateTime.UtcNow)});

            var subj = new HashCompatibilityProvider(versionHistory, null);

            return subj.IsInSha1CompatibilityMode();
        }
    }
}