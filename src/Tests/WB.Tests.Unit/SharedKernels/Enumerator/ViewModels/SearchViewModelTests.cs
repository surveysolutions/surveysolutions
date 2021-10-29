using System.Linq;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using MvvmCross.Views;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(SearchViewModel))]
    public class SearchViewModelTests : MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void Setup()
        {
            base.Setup();
            
            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);
            Ioc.RegisterType<ThrottlingViewModel>(() => Create.ViewModel.ThrottlingViewModel());
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }

        [Test]
        public async Task when_no_need_more_interviews_for_assignment_then_search_should_not_show_it_in_search_results()
        {
            var assignmentsStorage = Create.Storage.AssignmentDocumentsInmemoryStorage();
            var assignment = Create.Entity.AssignmentDocument(id: 1, quantity: 1, interviewsCount: 1).WithTitle("assignmet #1").Build();
            assignmentsStorage.Store(assignment);

            var viewModelFactory = new Mock<IInterviewViewModelFactory>();

            var searchViewModel = Create.ViewModel.SearchViewModel(assignmentsRepository: assignmentsStorage, viewModelFactory: viewModelFactory.Object);

            //act
            await searchViewModel.SearchAsync("assign");

            //assert
            CollectionAssert.IsEmpty(searchViewModel.UiItems);
            CollectionAssert.DoesNotContain(searchViewModel.UiItems, assignment);
            viewModelFactory.Verify(f => f.GetDashboardAssignment(It.IsAny<AssignmentDocument>()), Times.Never());
        }

        [Test]
        public async Task when_need_more_interviews_for_assignment_then_search_should_show_it_in_search_results()
        {
            var assignmentsStorage = Create.Storage.AssignmentDocumentsInmemoryStorage();
            var assignment = Create.Entity.AssignmentDocument(id: 1, quantity: 1, interviewsCount: 0).WithTitle("assignmet #1").Build();
            assignmentsStorage.Store(assignment);

            var dashboardItem = Mock.Of<IDashboardItem>();

            var viewModelFactory = new Mock<IInterviewViewModelFactory>();
            viewModelFactory.Setup(s => s.GetDashboardAssignment(It.Is<AssignmentDocument>(a => a.Id == assignment.Id))).Returns(dashboardItem);

            var searchViewModel = Create.ViewModel.SearchViewModel(assignmentsRepository: assignmentsStorage, viewModelFactory: viewModelFactory.Object);

            //act
            await searchViewModel.SearchAsync("assign");

            //assert
            CollectionAssert.IsNotEmpty(searchViewModel.UiItems);
            Assert.That(searchViewModel.UiItems.Count, Is.EqualTo(1));
            Assert.That(searchViewModel.UiItems.Single(), Is.EqualTo(dashboardItem));
            viewModelFactory.Verify(f => f.GetDashboardAssignment(It.IsAny<AssignmentDocument>()), Times.Once);
        }
    }
}
