using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_remove_answer_from_parent_cascading_question : InterviewTestsContext
    {
        private Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();

        };

        private Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {

                Setup.MockedServiceLocator();
                var questionnaireId = Guid.Parse("10000000000000000000000000000000");
                userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
                parentCascadingQuestion = Guid.Parse("22222222222222222222222222222222");
                childCascadingQuestionId = Guid.Parse("21111111111111111111111111111111");
                childOfChildCascadingQuestionId = Guid.Parse("31111111111111111111111111111111");

                QuestionnaireDocument questionnaire = Create.QuestionnaireDocument(id: questionnaireId,
                    children: new IComposite[]
                    {
                        Create.SingleQuestion(id: parentCascadingQuestion, variable: "par",
                            options: new List<Answer>() {Create.Answer("1", 1), Create.Answer("2", 2)}),
                        Create.SingleQuestion(id: childCascadingQuestionId, variable: "chil",
                            cascadeFromQuestionId: parentCascadingQuestion,
                            options: new List<Answer>() {Create.Answer("11", 11, 1), Create.Answer("21", 21, 2)}),
                        Create.SingleQuestion(id: childOfChildCascadingQuestionId, variable: "chilchil",
                            cascadeFromQuestionId: childCascadingQuestionId,
                            options: new List<Answer>() {Create.Answer("111", 111, 11), Create.Answer("211", 211, 21)})
                    });

                interview = SetupInterview(questionnaire);
                interview.AnswerSingleOptionQuestion(userId, parentCascadingQuestion, new decimal[0],
                    DateTime.Now, 2);

                interview.AnswerSingleOptionQuestion(userId, childCascadingQuestionId, new decimal[0],
                    DateTime.Now, 21);

                interview.AnswerSingleOptionQuestion(userId, childOfChildCascadingQuestionId, new decimal[0],
                 DateTime.Now, 211);
                eventContext = new EventContext();

                interview.RemoveAnswer(parentCascadingQuestion, new decimal[0], userId, DateTime.Now);
                return true;
            });

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_raise_AnswerRemoved_event_for_parent_question = () =>
            eventContext.ShouldContainEvent<AnswerRemoved>(@event
                => @event.QuestionId == parentCascadingQuestion && !@event.RosterVector.Any());

        It should_raise_QuestionsDisabled_event_for_child_question = () =>
            eventContext.ShouldContainEvent<QuestionsDisabled>(@event
                => @event.Questions.Count(q => q.Id == childCascadingQuestionId && !q.RosterVector.Any()) > 0);

        It should_raise_QuestionsDisabled_event_for_child_of_child_question = () =>
           eventContext.ShouldContainEvent<QuestionsDisabled>(@event
               => @event.Questions.Count(q => q.Id == childOfChildCascadingQuestionId && !q.RosterVector.Any()) > 0);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid parentCascadingQuestion;
        private static Guid childCascadingQuestionId;
        private static Guid childOfChildCascadingQuestionId;
        private static AppDomainContext appDomainContext;
        private static bool result;
    }
}