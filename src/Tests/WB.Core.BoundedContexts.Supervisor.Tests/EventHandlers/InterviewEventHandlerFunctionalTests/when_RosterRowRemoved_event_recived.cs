using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers.InterviewEventHandlerFunctionalTests
{
    internal class when_RosterRowRemoved_event_recived : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");
            viewState = CreateViewWithSequenceOfInterviewData();
            viewState.Document.Levels.Add("0", new InterviewLevel(rosterScopeId, null, new decimal[0]));

            var questionnaireRosterStructure = CreateQuestionnaireRosterStructure(rosterScopeId, rosterGroupId);

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure);
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new RosterRowDeleted(rosterGroupId, new decimal[0], 0)));

        It should_interview_levels_count_be_equal_to_0 = () =>
            viewState.Document.Levels.Keys.Count.ShouldEqual(0);

        It should_interview_level_with_id_0_be_present = () =>
            viewState.Document.Levels.Keys.ShouldNotContain("0");

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static ViewWithSequence<InterviewData> viewState;
        private static Guid rosterGroupId;
        private static Guid rosterScopeId;
    }
}
