using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore;
using Machine.Specifications;

using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    public class when_section_get_disable_event : SectionsViewModelTestContext
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
            questionnaire.GroupsWithFirstLevelChildrenAsReferences[sectionAId] = new GroupModel { Id = sectionAId, Title = "A" };
            questionnaire.GroupsWithFirstLevelChildrenAsReferences[sectionBId] = new GroupModel { Id = sectionBId, Title = "B" };

            var interview = Substitute.For<IStatefulInterview>();
            interview.IsEnabled(Moq.It.IsAny<Identity>()).ReturnsForAnyArgs(true);

            viewModel = CreateSectionsViewModel(questionnaire, interview);
            navigationState = Substitute.For<NavigationState>();
            viewModel.Init("", "", navigationState);

            firstSelectedSection = viewModel.Sections[0];
            secondSection = viewModel.Sections[1];
            secondSection.SectionIdentity = new Identity(sectionBId, Empty.RosterVector);
        };

        Because of = () =>
        {
            viewModel.Handle(new GroupsDisabled(new[] { new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(sectionBId, new decimal[0]) }));
        };

        It should_contains_only_one_section = () =>
            viewModel.Sections.Count.ShouldEqual(1);

        It should_contains_only_first_section = () =>
            viewModel.Sections.Single().ShouldEqual(firstSelectedSection);



        static SideBarSectionsViewModel viewModel;
        static SideBarSectionViewModel firstSelectedSection;
        static NavigationState navigationState;
        static Guid sectionBId;
        static Guid sectionAId;
        static SideBarSectionViewModel secondSection;
    }
}