using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SectionsViewModelTests
{
    internal class when_sections_model_has_disabled_items : SectionsViewModelTestContext
    {
        Establish context = () =>
        {
            //var questionnaire = Create.QuestionnaireModel();
            //questionnaire.GroupsHierarchy = new List<GroupsHierarchyModel>
            //{
            //    CreateGroupsHierarchyModel(sectionAId, "A"),
            //    CreateGroupsHierarchyModel(sectionBId, "B"),
            //    CreateGroupsHierarchyModel(sectionCId, "C"),
            //    CreateGroupsHierarchyModel(sectionDId, "D")
            //};

            //questionnaire.GroupsWithFirstLevelChildrenAsReferences = new Dictionary<Guid, GroupModel>
            //{
            //    { sectionAId, new GroupModel { Id = sectionAId, Title = "A" }},
            //    { sectionBId, new GroupModel { Id = sectionBId, Title = "B" }},
            //    { sectionCId, new GroupModel { Id = sectionCId, Title = "C" }},
            //    { sectionDId, new GroupModel { Id = sectionDId, Title = "D" }},
            //};

            var interview = Substitute.For<IStatefulInterview>();

            interview.IsEnabled(Arg.Is<Identity>(x => x.Id == sectionAId)).Returns(true);
            interview.IsEnabled(Arg.Is<Identity>(x => x.Id == sectionBId)).Returns(false);
            interview.IsEnabled(Arg.Is<Identity>(x => x.Id == sectionCId)).Returns(false);
            interview.IsEnabled(Arg.Is<Identity>(x => x.Id == sectionDId)).Returns(false);

            var questionnaire = new Mock<IQuestionnaire>();
            viewModel = CreateSectionsViewModel(questionnaire.Object, interview);
            navigationState = Substitute.For<NavigationState>();
            viewModel.Init("", "", navigationState);

            viewModel.Handle(Create.Event.GroupsDisabled(sectionBId, Empty.RosterVector));
            viewModel.Handle(Create.Event.GroupsDisabled(sectionCId, Empty.RosterVector));
            viewModel.Handle(Create.Event.GroupsDisabled(sectionDId, Empty.RosterVector));

            interview.IsEnabled(Arg.Is<Identity>(x => x.Id == sectionDId)).Returns(true);
        };

        Because of = () =>
        {
            viewModel.Handle(Create.Event.GroupsEnabled(sectionDId, Empty.RosterVector));
        };

        It should_contains_two_sections_and_complete_button = () =>
            viewModel.Sections.Count.ShouldEqual(2 + 1);

        It should_add_section_D_at_second_position = () =>
        {
            viewModel.Sections.ElementAt(1).SectionIdentity.Id.Equals(sectionDId);
            viewModel.Sections.ElementAt(1).SectionIdentity.RosterVector.Equals(Empty.RosterVector);
            viewModel.Sections.ElementAt(1).Title.ShouldEqual("D");
        };

        static SideBarSectionsViewModel viewModel;
        static NavigationState navigationState;

        static readonly Guid sectionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid sectionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static readonly Guid sectionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static readonly Guid sectionDId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}