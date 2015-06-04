using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_numeric_question_with_invalid_answer_and_all_questions_were_valid_before_answer : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionA = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                var questionB = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

                var interview = SetupInterview(
                    Create.QuestionnaireDocument(children: new[]
                    {
                        Create.Chapter(children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(id: questionA, variable: "a", validationExpression: "a > 0"),
                            Create.NumericIntegerQuestion(id: questionB, variable: "b", validationExpression: "b > 0"),
                        }),
                    }),
                    events: new object[]
                    {
                        Create.Event.NumericIntegerQuestionAnswered(questionId: questionA, answer: 1),
                        Create.Event.NumericIntegerQuestionAnswered(questionId: questionB, answer: 2),
                    });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), questionA, Empty.RosterVector, DateTime.Now, -3);

                    return new InvokeResult
                    {
                        AnswersDeclaredValidEventCount = eventContext.Count<AnswersDeclaredValid>(),
                        AnswersDeclaredInvalidEventCount = eventContext.Count<AnswersDeclaredInvalid>(),
                        AnswersDeclaredInvalidQuestionIds = eventContext.GetSingleEvent<AnswersDeclaredValid>().Questions.Select(identity => identity.Id).ToArray(),
                    };
                }
            });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_not_raise_AnswersDeclaredValid_event = () =>
            result.AnswersDeclaredValidEventCount.ShouldEqual(1);

        It should_raise_AnswersDeclaredInvalid_event = () =>
            result.AnswersDeclaredInvalidEventCount.ShouldEqual(0);

        It should_raise_AnswersDeclaredInvalid_event_with_answered_question_id_only = () =>
            result.AnswersDeclaredInvalidQuestionIds.ShouldContainOnly(Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"));

        private static AppDomainContext appDomainContext;
        private static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public int AnswersDeclaredValidEventCount { get; set; }
            public int AnswersDeclaredInvalidEventCount { get; set; }
            public Guid[] AnswersDeclaredInvalidQuestionIds { get; set; }
        }
    }
}