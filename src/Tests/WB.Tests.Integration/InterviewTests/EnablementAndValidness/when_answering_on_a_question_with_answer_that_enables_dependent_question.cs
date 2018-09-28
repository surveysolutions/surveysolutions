using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_on_a_question_with_answer_that_enables_dependent_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        public void BecauseOf() => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            SetUp.MockedServiceLocator();

            var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var dependentQuestionId = Guid.Parse("22222222222222222222222222222222");

            var interview = SetupInterviewWithExpressionStorage(questionnaireDocument: Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Abc.Create.Entity.NumericIntegerQuestion(answeredQuestionId, "q1"),
                Abc.Create.Entity.NumericIntegerQuestion(dependentQuestionId, enablementCondition: "q1 > 0"),
            }));

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(Abc.Create.Command.AnswerNumericIntegerQuestionCommand(questionId: answeredQuestionId, answer: 1));

                return new InvokeResults
                {
                    WasQuestionsEnabledEventPublishedForDependentQuestion =
                        eventContext.GetSingleEventOrNull<QuestionsEnabled>()?.Questions.Any(question => question.Id == dependentQuestionId) ?? false,
                };
            }
        });

        [NUnit.Framework.Test] public void should_enable_dependent_question () =>
            results.WasQuestionsEnabledEventPublishedForDependentQuestion.Should().BeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasQuestionsEnabledEventPublishedForDependentQuestion { get; set; }
        }
    }
}
