using System;
using System.Linq;
using Main.Core.Entities.Composite;
using MvvmCross.Core.Views;
using MvvmCross.Platform.Core;
using MvvmCross.Test.Core;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(SideBarSectionsViewModel))]
    public class SidebarSectionsViewModelTests: MvxIoCSupportingTest
    {
        [Test]
        public void When_section_enabled_Then_new_view_model_should_be_added_with_specified_index()
        {
            base.Setup();

            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(dispatcher);

            //arrange
            var section1Id = Guid.Parse("11111111111111111111111111111111");
            var disabledSectionId = Guid.Parse("22222222222222222222222222222222");
            var section3Id = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.Group(section1Id),
                Create.Entity.Group(disabledSectionId),
                Create.Entity.Group(section3Id)
            });

            var eventRegistry = Create.Service.LiteEventRegistry();

            var interview = Abc.Setup.StatefulInterview(questionnaire);
            interview.Apply(Create.Event.GroupsDisabled(disabledSectionId, RosterVector.Empty));

            var viewModel = Create.ViewModel.SidebarSectionsViewModel(questionnaire, interview, eventRegistry);
            var viewModelsWithoutDisabled = viewModel.AllVisibleSections.ToList();

            //act
            interview.Apply(Create.Event.GroupsEnabled(disabledSectionId, RosterVector.Empty));
            viewModel.Handle(Create.Event.GroupsEnabled(disabledSectionId, RosterVector.Empty));
            //assert
            var addedDisabledSectionViewModel = viewModel.AllVisibleSections.Except(viewModelsWithoutDisabled).Single();

            Assert.That(viewModel.AllVisibleSections.Count, Is.EqualTo(5));
            Assert.That(viewModel.AllVisibleSections.ToList().IndexOf(addedDisabledSectionViewModel), Is.EqualTo(2));
        }
    }
}
