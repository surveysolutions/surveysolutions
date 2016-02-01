using System;
using System.Collections.Generic;
using Machine.Specifications;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SectionsViewModelTests
{
    internal class when_section_is_expanded_while_other_section_is_expanded : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            sectionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            sectionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaire = Create.QuestionnaireModel();
            questionnaire.GroupsHierarchy = new List<GroupsHierarchyModel>
            {
                CreateGroupsHierarchyModel(sectionAId, "A"),
                CreateGroupsHierarchyModel(sectionBId, "B")
            };
            questionnaire.GroupsWithFirstLevelChildrenAsReferences = new Dictionary<Guid, GroupModel>();
            questionnaire.GroupsWithFirstLevelChildrenAsReferences[sectionAId] = new GroupModel{Id = sectionAId, Title = "A"};
            questionnaire.GroupsWithFirstLevelChildrenAsReferences[sectionBId] = new GroupModel{Id = sectionBId, Title = "B"};

            var interview = Substitute.For<IStatefulInterview>();
            interview.IsEnabled(Moq.It.IsAny<Identity>()).ReturnsForAnyArgs(true);

            viewModel = CreateSectionsViewModel(questionnaire, interview);
            navigationState = Substitute.For<NavigationState>();
            viewModel.Init("", "", navigationState);
            
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
            navigationState.ScreenChanged += Raise.Event<GroupChanged>(new ScreenChangedEventArgs { TargetGroup = secondSection.SectionIdentity });
        };

        It should_collapse_previous_group = () => firstSelectedSection.Expanded.ShouldBeFalse();

        It should_expand_navigated_to_group = () => secondSection.Expanded.ShouldBeTrue();

        static SideBarSectionsViewModel viewModel;
        static SideBarSectionViewModel firstSelectedSection;
        static NavigationState navigationState;
        static Guid sectionBId;
        static Guid sectionAId;
        static SideBarSectionViewModel secondSection;
    }
}