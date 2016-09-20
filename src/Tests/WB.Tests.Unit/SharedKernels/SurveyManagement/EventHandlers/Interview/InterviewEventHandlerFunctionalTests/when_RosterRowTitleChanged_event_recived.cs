using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    class when_RosterRowTitleChanged_event_recived : InterviewEventHandlerFunctionalTestContext
    {

        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");
            viewState = CreateViewWithSequenceOfInterviewData();
            viewState.Levels.Add("0", new InterviewLevel(new ValueVector<Guid>{ rosterScopeId }, null, new decimal[0]));

            var questionnaireRosterScopes = CreateQuestionnaireRosterScopes(rosterScopeId, rosterGroupId);

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterScopes);
        };
        
        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(Create.Event.RosterInstancesTitleChanged(rosterGroupId, rosterTitle: rosterTitle)));


        It should_roster_title_be_equal_to_rosterTitle = () =>
            viewState.Levels["0"].RosterRowTitles[rosterGroupId].ShouldEqual(rosterTitle);

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid rosterGroupId;
        private static Guid rosterScopeId;
        private static string rosterTitle = "new roster title";
    }
}
