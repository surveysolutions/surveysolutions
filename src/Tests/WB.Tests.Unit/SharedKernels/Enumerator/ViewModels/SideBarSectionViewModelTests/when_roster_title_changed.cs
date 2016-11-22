using System;
using Machine.Specifications;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
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
            sectionIdentity = new Identity(rosterGroupId, new[] { 0m });
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new[]
            {
                Create.Entity.FixedRoster(sectionIdentity.Id, title: "group title", fixedTitles: new[] {Create.Entity.FixedTitle(0, "rosterTitle")})
            });
            var plainQuestionnaires = Create.Entity.PlainQuestionnaire(questionnaire);

            var interview = Setup.StatefulInterview(questionnaire);
            
            viewModel = CreateViewModel(questionnaire: plainQuestionnaires, interview: interview);
            viewModel.Init("", NavigationIdentity.CreateForGroup(sectionIdentity),
                Substitute.For<SideBarSectionsViewModel>(), null, Substitute.For<GroupStateViewModel>(),
                Create.Other.NavigationState());

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

