using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answer_on_question_triggers_validation_evaluation_of_unanswered_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var questionnaireId = Guid.Parse("10000000000000000000000000000000");
                var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

                var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
                var dependentOnAnsweredQuestionId = Guid.Parse("22222222222222222222222222222222");

                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(answeredQuestionId, "q1"),
                    Abc.Create.Entity.NumericIntegerQuestion(dependentOnAnsweredQuestionId, "q2", validationExpression: "q1 + q2 > 0")
               );

                var interview = SetupInterview(questionnaireDocument, new List<object>()
                {
                    Abc.Create.Event.AnswersDeclaredInvalid(IntegrationCreate.FailedValidationCondition(Abc.Create.Identity(dependentOnAnsweredQuestionId))),
                    Abc.Create.Event.NumericIntegerQuestionAnswered(
                        dependentOnAnsweredQuestionId, null, 1, null, null
                    )
                });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, answeredQuestionId, new decimal[] { }, DateTime.Now, 1);

                    result.AnswerIsValid = GetFirstEventByType<AnswersDeclaredValid>(eventContext.Events).Questions.FirstOrDefault(q => q.Id == dependentOnAnsweredQuestionId) != null;
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_not_enable_groupId () =>
            results.AnswerIsValid.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool AnswerIsValid { get; set; }
        } 
    }
}
