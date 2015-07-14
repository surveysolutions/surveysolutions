using System;
using System.Collections.Generic;
using System.Web.WebSockets;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    public class when_section_is_expanded_while_other_section_is_expanded : SectionsViewModelTestContext
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
            navigationState.GroupChanged += Raise.Event<GroupChanged>(new GroupChangedEventArgs { TargetGroup = secondSection.SectionIdentity });
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