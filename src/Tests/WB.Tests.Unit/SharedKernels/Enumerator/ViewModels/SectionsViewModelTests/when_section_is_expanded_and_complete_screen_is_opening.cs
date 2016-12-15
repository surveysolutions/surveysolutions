using System;
using System.Collections.Generic;

using Machine.Specifications;
using Moq;
using NSubstitute;

using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SectionsViewModelTests
{
    internal class when_section_is_expanded_and_complete_screen_is_opening : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            sectionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            sectionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var interview = Substitute.For<IStatefulInterview>();
            interview.IsEnabled(Moq.It.IsAny<Identity>()).ReturnsForAnyArgs(true);

            var questionnaire = Mock.Of<IQuestionnaire>(_ => _.GetAllSections() == listOfSections);
            viewModel = CreateSectionsViewModel(questionnaire, interview);
            navigationState = Substitute.For<NavigationState>();
            viewModel.Init("", Create.Entity.QuestionnaireIdentity(), navigationState);

            firstSelectedSection = viewModel.Sections[0];
            secondSection = viewModel.Sections[1];

            firstSelectedSection.Expanded = true;
            firstSelectedSection.IsSelected = true;
            firstSelectedSection.IsCurrent = true;
            firstSelectedSection.SectionIdentity = new Identity(sectionAId, Empty.RosterVector);
            secondSection.SectionIdentity = new Identity(sectionBId, Empty.RosterVector);
        };

        Because of = () =>
        {
            navigationState.ScreenChanged += Raise.Event<GroupChanged>(Create.Entity.ScreenChangedEventArgs(targetStage: ScreenType.Complete));
        };

        It should_collapse_previous_group = () => 
            firstSelectedSection.Expanded.ShouldBeFalse();

        It should_not_select_previous_group = () => 
            firstSelectedSection.IsSelected.ShouldBeFalse();

        It should_expand_second_section = () => 
            secondSection.Expanded.ShouldBeFalse();

        It should_not_select_second_section = () => 
            secondSection.IsSelected.ShouldBeFalse();

        static SideBarSectionsViewModel viewModel;
        static NavigationState navigationState;
        static SideBarSectionViewModel firstSelectedSection;
        static SideBarSectionViewModel secondSection;
        static Guid sectionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Guid sectionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly List<Guid> listOfSections = new List<Guid> { sectionAId, sectionBId };
    }
}