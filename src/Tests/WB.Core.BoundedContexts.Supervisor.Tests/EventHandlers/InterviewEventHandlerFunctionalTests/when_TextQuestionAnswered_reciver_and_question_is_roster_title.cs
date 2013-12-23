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
    internal class when_TextQuestionAnswered_reciver_and_question_is_roster_title : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");
            headQuestionId = Guid.Parse("13333333333333333333333333333333");
            viewState = CreateViewWithSequenceOfInterviewData();
            var questionnaireRosterStructure = CreateQuestionnaireRosterStructure(rosterScopeId,
                new Dictionary<Guid, Guid?>() { { rosterGroupId, headQuestionId } });

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure);
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new RosterRowAdded(rosterGroupId, new decimal[0], 0, null)));
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new TextQuestionAnswered(Guid.NewGuid(), headQuestionId, new decimal[] { 0 }, DateTime.Now,
                    helloRoster)));

        It should_roster_title_be_equal_to_helloRoster = () =>
            viewState.Document.Levels["0"].RosterRowTitles[rosterGroupId].ShouldEqual(helloRoster);

        It should_answer_on_head_question_be_equal_to_helloRoster = () =>
            viewState.Document.Levels["0"].GetAllQuestions().First(q => q.Id == headQuestionId).Answer.ShouldEqual(helloRoster);

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static ViewWithSequence<InterviewData> viewState;
        private static Guid rosterGroupId;
        private static Guid rosterScopeId;
        private static Guid headQuestionId;
        private static string helloRoster = "hello roster";
    }
}
