﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers.InterviewEventHandlerFunctionalTests
{
    internal class when_MultipleOptionsQuestionAnswered_recived_and_answered_question_is_roster_title : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");
            rosterTitleQuestionId = Guid.Parse("13333333333333333333333333333333");
            viewState = CreateViewWithSequenceOfInterviewData();
            var questionnaireRosterStructure = CreateQuestionnaireRosterStructure(rosterScopeId,
                new Dictionary<Guid, RosterTitleQuestionDescription>
                {
                    {
                        rosterGroupId,
                        new RosterTitleQuestionDescription(rosterTitleQuestionId,
                            new Dictionary<decimal, string> { { 1, "one" }, { 2, "two" }, { 3, "three" } })
                    }
                });

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure);
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new RosterRowAdded(rosterGroupId, new decimal[0], 0, null)));
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new MultipleOptionsQuestionAnswered(Guid.NewGuid(), rosterTitleQuestionId, new decimal[] { 0 }, DateTime.Now,
                    selectedOptions)));

        It should_roster_title_be_equal_to_recived_answer_text_representation = () =>
            viewState.Document.Levels["0"].RosterRowTitles[rosterGroupId].ShouldEqual(selectedOptionsText);

        It should_answer_on_head_question_be_equal_to_recived_answer = () =>
            viewState.Document.Levels["0"].GetAllQuestions().First(q => q.Id == rosterTitleQuestionId).Answer.ShouldEqual(selectedOptions);

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static ViewWithSequence<InterviewData> viewState;
        private static Guid rosterGroupId;
        private static Guid rosterScopeId;
        private static Guid rosterTitleQuestionId;
        private static decimal[] selectedOptions = new decimal[] { 1, 3 };
        private static string selectedOptionsText = "one, three";
    }
}
