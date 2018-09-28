using System;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_on_a_question_with_answer_that_makes_dependent_question_invalid : InterviewTestsContext
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

            var interview = SetupInterviewWithExpressionStorage(Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(answeredQuestionId, "q1"),
                    Create.Entity.NumericIntegerQuestion(dependentQuestionId, "q2",
                        validationConditions: Create.Entity.ValidationCondition("q1 != q2", null).ToEnumerable())
                }), new object[]
                {
                    Create.Event.NumericIntegerQuestionAnswered(
                        dependentQuestionId, null, 1, null, null
                    )
                });

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: answeredQuestionId, answer: 1));

                return new InvokeResults
                {
                    WasAnswersDeclaredInvalidEventPublishedForDependentQuestion =
                        eventContext
                            .GetSingleEventOrNull<AnswersDeclaredInvalid>()?
                            .FailedValidationConditions
                            .ContainsKey(Create.Identity(dependentQuestionId))
                        ?? false
                };
            }
        });

        [NUnit.Framework.Test] public void should_mark_dependent_question_as_invalid () =>
            results.WasAnswersDeclaredInvalidEventPublishedForDependentQuestion.Should().BeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasAnswersDeclaredInvalidEventPublishedForDependentQuestion { get; set; }
        }
    }
}
