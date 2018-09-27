using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_on_question_condition_expression_throws_exception : InterviewTestsContext
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
                        Abc.Create.Entity.NumericIntegerQuestion(question2Id, "q2", enablementCondition: "1/q1 == 1")
                    ),
                    events: new List<object>
                    {
                        Abc.Create.Event.NumericIntegerQuestionAnswered(
                            question1Id, null, 1, null, null
                        ),
                        Abc.Create.Event.QuestionsEnabled(new []
                        {
                            Abc.Create.Identity(question1Id),
                            Abc.Create.Identity(question2Id)
                        })
                    });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(actorId, question1Id, RosterVector.Empty, DateTime.Now, 0);

                    result.Questions2DisabledEventWasFound  = 
                        eventContext.Events.Any(b => b.Payload is QuestionsDisabled) &&
                        GetFirstEventByType<QuestionsDisabled>(eventContext.Events).Questions.Any(q => q.Id == question2Id);

                    result.Questions2EnabledEventWasFound =
                        eventContext.Events.Any(b => b.Payload is QuestionsEnabled) &&
                        GetFirstEventByType<QuestionsEnabled>(eventContext.Events).Questions.Any(q => q.Id == question2Id);
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_not_enable_question2Id () =>
            results.Questions2EnabledEventWasFound.Should().BeFalse();

        [NUnit.Framework.Test] public void should_disable_question2Id_because_of_calculation_error () =>
            results.Questions2DisabledEventWasFound.Should().BeTrue();

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
            public bool Questions2EnabledEventWasFound { get; set; }
            public bool Questions2DisabledEventWasFound { get; set; }
        } 
    }
}
