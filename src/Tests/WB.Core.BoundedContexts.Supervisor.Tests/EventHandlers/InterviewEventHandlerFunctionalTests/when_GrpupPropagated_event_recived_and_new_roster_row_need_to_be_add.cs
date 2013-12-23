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
    internal class when_GrpupPropagated_event_recived_and_new_roster_row_need_to_be_add : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");
            viewState = CreateViewWithSequenceOfInterviewData();
            var questionnaireRosterStructure = CreateQuestionnaireRosterStructure(rosterScopeId, rosterGroupId);

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure);
        };

        Because of = () =>
            viewState= interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new GroupPropagated(rosterGroupId, new decimal[0], 1)));

        It should_interview_levels_count_be_equal_to_1 = () =>
            viewState.Document.Levels.Keys.Count.ShouldEqual(1);

        It should_interview_level_with_id_0_be_present = () =>
            viewState.Document.Levels.Keys.ShouldContain("0");

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static ViewWithSequence<InterviewData> viewState;
        private static Guid rosterGroupId;
        private static Guid rosterScopeId;
    }
}
