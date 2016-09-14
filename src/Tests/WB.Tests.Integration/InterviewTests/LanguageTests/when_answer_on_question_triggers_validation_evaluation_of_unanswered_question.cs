using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answer_on_question_triggers_validation_evaluation_of_unanswered_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var questionnaireId = Guid.Parse("10000000000000000000000000000000");
                var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

                var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
                var dependentOnAnsweredQuestionId = Guid.Parse("22222222222222222222222222222222");

                Setup.MockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.NumericIntegerQuestion(answeredQuestionId, "q1"),
                    Create.NumericIntegerQuestion(dependentOnAnsweredQuestionId, "q2", validationExpression: "q1 + q2 > 0")
               );

                var interview = SetupInterview(questionnaireDocument, new List<object>()
                {
                    Create.Event.AnswersDeclaredInvalid(Create.Identity(dependentOnAnsweredQuestionId)),
                    Create.Event.NumericIntegerQuestionAnswered(dependentOnAnsweredQuestionId, 1)
                });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, answeredQuestionId, new decimal[] { }, DateTime.Now, 1);

                    result.AnswerIsValid = GetFirstEventByType<AnswersDeclaredValid>(eventContext.Events).Questions.FirstOrDefault(q => q.Id == dependentOnAnsweredQuestionId) != null;
                }

                return result;
            });

        It should_not_enable_groupId = () =>
            results.AnswerIsValid.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool AnswerIsValid { get; set; }
        } 
    }
}
