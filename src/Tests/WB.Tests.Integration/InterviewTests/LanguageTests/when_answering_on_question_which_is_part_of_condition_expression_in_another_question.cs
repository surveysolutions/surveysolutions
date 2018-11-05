using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_on_question_which_is_part_of_condition_expression_in_another_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {                
                SetUp.MockedServiceLocator();

                var questionnaireId = Guid.Parse("00000000000000000000000000000000");
                var actorId = Guid.Parse("99999999999999999999999999999999");
                var question1Id = Guid.Parse("11111111111111111111111111111111");
                var question2Id = Guid.Parse("22222222222222222222222222222222");

                var interview = SetupInterview(
                    Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                        Abc.Create.Entity.NumericIntegerQuestion(question1Id, "q1"),
                        Abc.Create.Entity.NumericIntegerQuestion(question2Id, "q2", enablementCondition: "q1 > 3")
                    ),
                    events: new List<object>
                    {
                        Abc.Create.Event.NumericIntegerQuestionAnswered(
                            question1Id, null, 1, null, null
                        ),
                        Abc.Create.Event.NumericIntegerQuestionAnswered(
                            question1Id, null, 2, null, null
                        ),
                        Abc.Create.Event.NumericIntegerQuestionAnswered(
                            question1Id, null, 3, null, null
                        )
                    });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(actorId, question1Id, new decimal[0], DateTime.Now, 4);

                    result.Questions2Enabled = GetFirstEventByType<QuestionsEnabled>(eventContext.Events).Questions.FirstOrDefault(q => q.Id == question2Id) != null;
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_enable_second_question () =>
            results.Questions2Enabled.Should().BeTrue();

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
            public bool Questions2Enabled { get; set; }
        }
    }
}
