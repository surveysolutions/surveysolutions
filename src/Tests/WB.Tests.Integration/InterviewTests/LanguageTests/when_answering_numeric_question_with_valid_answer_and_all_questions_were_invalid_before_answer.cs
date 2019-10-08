using System;
using System.Linq;

using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_numeric_question_with_valid_answer_and_all_questions_were_invalid_before_answer : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionA = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                var questionB = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

                var interview = SetupInterview(
                    appDomainContext.AssemblyLoadContext,
                    Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {
                        Abc.Create.Entity.Group(children: new IComposite[]
                        {
                            Abc.Create.Entity.NumericIntegerQuestion(id: questionA, variable: "a", validationExpression: "a > 0"),
                            Abc.Create.Entity.NumericIntegerQuestion(id: questionB, variable: "b", validationExpression: "b > 0"),
                        }),
                    }),
                    events: new object[]
                    {
                        Abc.Create.Event.NumericIntegerQuestionAnswered(
                            questionA, answer: -1
                        ),
                        Abc.Create.Event.NumericIntegerQuestionAnswered(
                            questionB, answer: -2
                        ),
                        Abc.Create.Event.AnswersDeclaredInvalid(IntegrationCreate.FailedValidationCondition(Abc.Create.Identity(questionA))),
                        Abc.Create.Event.AnswersDeclaredInvalid(IntegrationCreate.FailedValidationCondition(Abc.Create.Identity(questionB))),
                    });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), questionA, RosterVector.Empty, DateTime.Now, 3);

                    return new InvokeResult
                    {
                        AnswersDeclaredValidEventCount = eventContext.Count<AnswersDeclaredValid>(),
                        AnswersDeclaredInvalidEventCount = eventContext.Count<AnswersDeclaredInvalid>(),
                        AnswersDeclaredValidQuestionIds = eventContext.GetSingleEvent<AnswersDeclaredValid>().Questions.Select(identity => identity.Id).ToArray(),
                    };
                }
            });

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [NUnit.Framework.Test] public void should_not_AnswersDeclaredInvalid_event () =>
            result.AnswersDeclaredInvalidEventCount.Should().Be(0);

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredValid_event () =>
            result.AnswersDeclaredValidEventCount.Should().Be(1);

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredValid_event_with_answered_question_id_only () =>
            result.AnswersDeclaredValidQuestionIds.Should().BeEquivalentTo(Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"));

        private static AppDomainContext appDomainContext;
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
