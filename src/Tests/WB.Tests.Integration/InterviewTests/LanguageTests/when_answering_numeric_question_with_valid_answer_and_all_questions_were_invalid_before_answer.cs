using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_numeric_question_with_valid_answer_and_all_questions_were_invalid_before_answer : InterviewTestsContext
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
                    Create.QuestionnaireDocumentWithOneChapter(children: new[]
                    {
                        Create.Chapter(children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(id: questionA, variable: "a", validationExpression: "a > 0"),
                            Create.NumericIntegerQuestion(id: questionB, variable: "b", validationExpression: "b > 0"),
                        }),
                    }),
                    events: new object[]
                    {
                        Create.Event.NumericIntegerQuestionAnswered(questionId: questionA, answer: -1),
                        Create.Event.NumericIntegerQuestionAnswered(questionId: questionB, answer: -2),
                        Create.Event.AnswersDeclaredInvalid(
                            new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>()
                            {
                                {
                                    Create.Identity(questionA),
                                    new List<FailedValidationCondition>() {new FailedValidationCondition(0)}
                                }
                            }),

                        Create.Event.AnswersDeclaredInvalid(
                            new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>()
                            {
                                {
                                    Create.Identity(questionB),
                                    new List<FailedValidationCondition>() {new FailedValidationCondition(0)}
                                }
                            })
                    });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), questionA, Empty.RosterVector, DateTime.Now, 3);

                    return new InvokeResult
                    {
                        AnswersDeclaredValidEventCount = eventContext.Count<AnswersDeclaredValid>(),
                        AnswersDeclaredInvalidEventCount = eventContext.Count<AnswersDeclaredInvalid>(),
                        AnswersDeclaredValidQuestionIds = eventContext.GetSingleEvent<AnswersDeclaredValid>().Questions.Select(identity => identity.Id).ToArray(),
                    };
                }
            });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_not_AnswersDeclaredInvalid_event = () =>
            result.AnswersDeclaredInvalidEventCount.ShouldEqual(0);

        It should_raise_AnswersDeclaredValid_event = () =>
            result.AnswersDeclaredValidEventCount.ShouldEqual(1);

        It should_raise_AnswersDeclaredValid_event_with_answered_question_id_only = () =>
            result.AnswersDeclaredValidQuestionIds.ShouldContainOnly(Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"));

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public int AnswersDeclaredValidEventCount { get; set; }
            public int AnswersDeclaredInvalidEventCount { get; set; }
            public Guid[] AnswersDeclaredValidQuestionIds { get; set; }
        }
    }
}