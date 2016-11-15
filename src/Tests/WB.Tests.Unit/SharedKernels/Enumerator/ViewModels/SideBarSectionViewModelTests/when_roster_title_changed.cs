using System;
using Machine.Specifications;
using Moq;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.SideBarSectionViewModelTests
{
    internal class when_roster_title_changed : SideBarSectionViewModelTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Mock.Of<IQuestionnaire>(_
               => _.HasGroup(rosterGroupId) == true
               && _.IsRosterGroup(rosterGroupId) == true
               && _.GetGroupTitle(rosterGroupId) == "group title");

            var interview = Substitute.For<IStatefulInterview>();

            viewModel = CreateViewModel(questionnaire: questionnaire, interview: interview);

            sectionIdentity = new Identity(rosterGroupId, new[]{0m});
            viewModel.Init("", NavigationIdentity.CreateForGroup(sectionIdentity), Substitute.For<SideBarSectionsViewModel>(), null, Substitute.For<GroupStateViewModel>(), Create.Other.NavigationState());

            viewModel.SectionIdentity = sectionIdentity;
        };

        Because of = () => viewModel.Title.Handle(Create.Event.RosterInstancesTitleChanged(rosterId: sectionIdentity.Id, 
            rosterTitle: "rosterTitle", 
            outerRosterVector: Empty.RosterVector,
            instanceId: 0m));

        It should_change_own_title = () => viewModel.Title.PlainText.ShouldEqual("group title - rosterTitle");

        static SideBarSectionViewModel viewModel;
        static Identity sectionIdentity;
        static readonly Guid rosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}

