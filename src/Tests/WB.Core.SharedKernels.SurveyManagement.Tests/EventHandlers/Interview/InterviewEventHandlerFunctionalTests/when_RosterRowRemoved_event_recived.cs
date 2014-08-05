using System;
using Machine.Specifications;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_RosterRowRemoved_event_recived : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");
            viewState = CreateViewWithSequenceOfInterviewData();
            viewState.Document.Levels.Add("0", new InterviewLevel(new ValueVector<Guid> { rosterScopeId }, null, new decimal[0]));

            var questionnaireRosterStructure = CreateQuestionnaireRosterStructure(rosterScopeId, rosterGroupId);

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure);
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new RosterRowRemoved(rosterGroupId, new decimal[0], 0)));

        It should_interview_levels_count_be_equal_to_1 = () =>
            viewState.Document.Levels.Keys.Count.ShouldEqual(1);

        It should_interview_level_with_id_0_be_present = () =>
            viewState.Document.Levels.Keys.ShouldNotContain("0");

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static ViewWithSequence<InterviewData> viewState;
        private static Guid rosterGroupId;
        private static Guid rosterScopeId;
    }
}
