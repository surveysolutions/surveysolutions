using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    [Ignore("KP-4381")]
    internal class when_answering_on_question_validation_expression_throws_exception : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireId = Guid.Parse("00000000000000000000000000000000");
                var actorId = Guid.Parse("99999999999999999999999999999999");
                var question1Id = Guid.Parse("11111111111111111111111111111111");
                var question2Id = Guid.Parse("22222222222222222222222222222222");

                var interview = SetupInterview(
                    Create.QuestionnaireDocument(questionnaireId,
                        Create.NumericIntegerQuestion(question1Id, "q1"),
                        Create.NumericIntegerQuestion(question2Id, "q2", validationExpression: "1/q1 == 1")
                    ),
                    events: new List<object>
                    {
                        Create.Event.QuestionsEnabled(new []
                        {
                            Create.Identity(question1Id),
                            Create.Identity(question2Id)
                        }),
                        Create.Event.AnswersDeclaredValid(new []
                        {
                            Create.Identity(question1Id),
                            Create.Identity(question2Id)                            
                        }),
                        Create.Event.NumericIntegerQuestionAnswered(question1Id, 1)
                    });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(actorId, question1Id, new decimal[0], DateTime.Now, 0);

                    result.Questions2ShouldBeDeclaredInvalid = 
                        HasEvent<AnswersDeclaredInvalid>(eventContext.Events) &&
                        GetFirstEventByType<AnswersDeclaredInvalid>(eventContext.Events).Questions.FirstOrDefault(q => q.Id == question2Id) != null;
                }

                return result;
            });

        It should_disable_second_question = () =>
            results.Questions2ShouldBeDeclaredInvalid.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool Questions2ShouldBeDeclaredInvalid { get; set; }
        } 
    }
}