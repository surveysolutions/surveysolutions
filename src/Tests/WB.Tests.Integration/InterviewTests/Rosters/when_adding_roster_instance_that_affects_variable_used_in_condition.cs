using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_adding_roster_instance_that_affects_variable_used_in_condition : InterviewTestsContext
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

        public void BecauseOf() => result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            SetUp.MockedServiceLocator();

            var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var listRosterId = Guid.Parse("22222222222222222222222222222222");
            Guid variableId = Guid.Parse("33333333333333333333333333333333");
            Guid singleOptionQuestionId = Guid.Parse("44444444444444444444444444444444");
            Guid numQuestionId = Guid.Parse("55555555555555555555555555555555");

            Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            var interview = SetupInterview(questionnaireDocument: Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextListQuestion(answeredQuestionId, variable: "q1"),
                Create.Entity.ListRoster(listRosterId, variable: "lst", rosterSizeQuestionId: answeredQuestionId, children: new IComposite[]
                {
                    Create.Entity.SingleOptionQuestion(singleOptionQuestionId, variable: "sgl", linkedToQuestionId: answeredQuestionId),
                    Create.Entity.Variable(variableId, VariableType.LongInteger, expression: "(long)sgl", variableName: "v1"),
                    Create.Entity.NumericIntegerQuestion(numQuestionId, variable: "num", enablementCondition: "v1 == null")
                })
            }));

            interview.AnswerTextListQuestion(userId, answeredQuestionId, RosterVector.Empty, DateTime.Now, new[] { Tuple.Create(1m, "A"), Tuple.Create(2m, "B") });
            interview.AnswerSingleOptionQuestion(userId, singleOptionQuestionId, Create.RosterVector(1), DateTime.Now, 2);

            using (var eventContext = new EventContext())
            {
                interview.AnswerTextListQuestion(userId, answeredQuestionId, RosterVector.Empty, DateTime.Now, new[] { Tuple.Create(1m, "A") });

                return new InvokeResults
                {
                    QuestionEnabledEventRaised = eventContext.GetSingleEventOrNull<QuestionsEnabled>()?.Questions?.Any(x => x.Id == numQuestionId) ?? false
                };
            }
        });

        [NUnit.Framework.Test]
        public void should_raise_question_enabled_event_if_related_question_has_answer_removed_and_variable_changed () 
            => result.QuestionEnabledEventRaised.Should().BeTrue();

        private static InvokeResults result;

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionEnabledEventRaised { get; set; }
        }
    }
}
