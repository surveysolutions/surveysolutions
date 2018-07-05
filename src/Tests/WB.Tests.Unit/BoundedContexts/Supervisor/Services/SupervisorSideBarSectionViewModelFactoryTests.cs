using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Properties;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorSideBarSectionViewModelFactory))]
    public class SupervisorSideBarSectionViewModelFactoryTests
    {
        [Test]
        public void should_initialize_with_resolve_text()
        {
            var serviceLocator = Mock.Of<IServiceLocator>(x => x.GetInstance<SideBarCompleteSectionViewModel>() ==
                                                               Create.ViewModel.SideBarCompleteSectionViewModel());

            var factory = new SupervisorSideBarSectionViewModelFactory(serviceLocator);
            var completeItem = factory.BuildCompleteItem(Create.Other.NavigationState(), Id.g1.FormatGuid());

            completeItem.Title.PlainText.Should().Be(InterviewDetails.Resolve);
        }
    }
}
