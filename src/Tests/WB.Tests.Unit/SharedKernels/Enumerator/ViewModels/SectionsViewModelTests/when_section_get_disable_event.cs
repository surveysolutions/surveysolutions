using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SectionsViewModelTests
{
    internal class when_section_get_disable_event : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            var interview = Substitute.For<IStatefulInterview>();
            interview.IsEnabled(Moq.It.IsAny<Identity>()).ReturnsForAnyArgs(true);

            var questionnaire = Mock.Of<IQuestionnaire>(_ => _.GetAllSections() == listOfSections);
            viewModel = CreateSectionsViewModel(questionnaire, interview);
            navigationState = Substitute.For<NavigationState>();
            viewModel.Init("", Create.Entity.QuestionnaireIdentity(), navigationState);

            firstSelectedSection = viewModel.Sections[0];
            secondSection = viewModel.Sections[1];
            secondSection.SectionIdentity = new Identity(sectionBId, Empty.RosterVector);
        };

        Because of = () =>
        {
            viewModel.Handle(new GroupsDisabled(new[] { new Identity(sectionBId, new decimal[0]) }));
        };

        It should_contains_only_one_section_and_complete_and_cover_button = () =>
            viewModel.Sections.Count.ShouldEqual(1+1+1);

        It should_contains_first_section = () =>
            viewModel.Sections.First().ShouldEqual(firstSelectedSection);

        static SideBarSectionsViewModel viewModel;
        static SideBarSectionViewModel firstSelectedSection;
        static NavigationState navigationState;
        static Guid sectionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Guid sectionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static SideBarSectionViewModel secondSection;
        private static readonly List<Guid> listOfSections = new List<Guid> { sectionAId, sectionBId };
    }
}