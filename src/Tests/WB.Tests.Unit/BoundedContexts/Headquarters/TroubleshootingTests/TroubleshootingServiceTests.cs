using System;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.TroubleshootingTests
{
    [TestFixture]
    internal class TroubleshootingServiceTests
    {
        [SetUp]
        public void SetupTests()
        {
            var serviceLocator = Stub<IServiceLocator>.WithNotEmptyValues;
            ServiceLocator.SetLocatorProvider(() => serviceLocator);
        }

        [Test]
        public void When_there_is_no_broken_packages_with_census_interviews()
        {
            var filter = new BrokenInterviewPackageFilter
            {
                QuestionnaireIdentity = "aaaa$1",
                FromProcessingDateTime = DateTime.Now.AddMonths(-1),
                ToProcessingDateTime = DateTime.Now.AddDays(1),
            };
            var brokenPackagesFactoryMock = new Mock<IBrokenInterviewPackagesViewFactory>();
            brokenPackagesFactoryMock
                .Setup(x => x.GetFilteredItems(It.IsAny<BrokenInterviewPackageFilter>()))
                .Returns(Create.Entity.BrokenInterviewPackagesView());

            var service = Create.Service.Troubleshooting(brokenPackagesFactory: brokenPackagesFactoryMock.Object);

            var message = service.GetCensusInterviewsMissingReason(filter.QuestionnaireIdentity, null,
                filter.FromProcessingDateTime.Value, filter.ToProcessingDateTime.Value);

            Assert.AreEqual(TroubleshootingMessages.MissingCensusInterviews_NoBrokenPackages_Message, message);
        }

        [Test]
        public void When_there_is_broken_package_with_census_interview()
        {
            var filter = new BrokenInterviewPackageFilter
            {
                QuestionnaireIdentity = "aaaa$1",
                FromProcessingDateTime = DateTime.Now.AddMonths(-1),
                ToProcessingDateTime = DateTime.Now.AddDays(1),
            };
            var brokenPackagesFactoryMock = new Mock<IBrokenInterviewPackagesViewFactory>();
            brokenPackagesFactoryMock
                .Setup(x => x.GetFilteredItems(It.IsAny<BrokenInterviewPackageFilter>()))
                .Returns(Create.Entity.BrokenInterviewPackagesView(Create.Entity.BrokenInterviewPackageView()));

            var service = Create.Service.Troubleshooting(brokenPackagesFactory: brokenPackagesFactoryMock.Object);

            var message = service.GetCensusInterviewsMissingReason(filter.QuestionnaireIdentity, null, 
                filter.FromProcessingDateTime.Value, filter.ToProcessingDateTime.Value);

            Assert.AreEqual(TroubleshootingMessages.MissingCensusInterviews_SomeBrokenPackages_Message, message);
        }
    }
}
