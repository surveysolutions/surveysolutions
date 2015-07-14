using System;
using System.Collections.Generic;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.SideBarSectionViewModelTests
{
    public class when_roster_title_changed : SideBarSectionViewModelTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            QuestionnaireModel questionnaire = Create.QuestionnaireModel();
            questionnaire.GroupsWithFirstLevelChildrenAsReferences = new Dictionary<Guid, GroupModel>();
            questionnaire.GroupsWithFirstLevelChildrenAsReferences[rosterGroupId] = new GroupModel { Title = "group title" };

            var interview = Substitute.For<IStatefulInterview>();

            viewModel = CreateViewModel(questionnaire: questionnaire, interview: interview);

            sectionIdentity = new Identity(rosterGroupId, new[]{0m});
            viewModel.Init("", sectionIdentity, null,  Create.NavigationState());

            viewModel.SectionIdentity = sectionIdentity;
        };

        Because of = () => viewModel.Handle(Create.RosterInstancesTitleChanged(rosterId: sectionIdentity.Id, 
            rosterTitle: "rosterTitle", 
            outerRosterVector: Empty.RosterVector,
            instanceId: 0m));

        It should_change_own_title = () => viewModel.Title.ShouldEqual("group title - rosterTitle");

        static SideBarSectionViewModel viewModel;
        static Identity sectionIdentity;
        static Guid rosterGroupId;
    }
}

