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
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SectionsViewModelTests
{
    internal class when_section_get_disable_event : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            sectionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            sectionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            //var questionnaire = Create.QuestionnaireModel();
            //questionnaire.GroupsHierarchy = new List<GroupsHierarchyModel>
            //{
            //    CreateGroupsHierarchyModel(sectionAId, "A"),
            //    CreateGroupsHierarchyModel(sectionBId, "B")
            //};
            //questionnaire.GroupsWithFirstLevelChildrenAsReferences = new Dictionary<Guid, GroupModel>();
            //questionnaire.GroupsWithFirstLevelChildrenAsReferences[sectionAId] = new GroupModel { Id = sectionAId, Title = "A" };
            //questionnaire.GroupsWithFirstLevelChildrenAsReferences[sectionBId] = new GroupModel { Id = sectionBId, Title = "B" };

            var interview = Substitute.For<IStatefulInterview>();
            interview.IsEnabled(Moq.It.IsAny<Identity>()).ReturnsForAnyArgs(true);

            var questionnaire = new Mock<IQuestionnaire>();
            viewModel = CreateSectionsViewModel(questionnaire.Object, interview);
            navigationState = Substitute.For<NavigationState>();
            viewModel.Init("", "", navigationState);

            firstSelectedSection = viewModel.Sections[0];
            secondSection = viewModel.Sections[1];
            secondSection.SectionIdentity = new Identity(sectionBId, Empty.RosterVector);
        };

        Because of = () =>
        {
            viewModel.Handle(new GroupsDisabled(new[] { new Identity(sectionBId, new decimal[0]) }));
        };

        It should_contains_only_one_section_and_complete_button = () =>
            viewModel.Sections.Count.ShouldEqual(2);

        It should_contains_first_section = () =>
            viewModel.Sections.First().ShouldEqual(firstSelectedSection);



        static SideBarSectionsViewModel viewModel;
        static SideBarSectionViewModel firstSelectedSection;
        static NavigationState navigationState;
        static Guid sectionBId;
        static Guid sectionAId;
        static SideBarSectionViewModel secondSection;
    }
}