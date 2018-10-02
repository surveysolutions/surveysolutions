using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_adding_roster_instance_that_affects_variable : InterviewTestsContext
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
            
            Guid userId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            var interview = SetupInterview(
                questionnaireDocument: Create.Entity.QuestionnaireDocumentWithOneChapter(
                    children: new IComposite[]
                    {
                        Create.Entity.TextListQuestion(answeredQuestionId, variable: "q1"),
                        Create.Entity.Roster(
                                rosterId: listRosterId,
                                variable: "lst",
                                rosterSizeQuestionId: answeredQuestionId,
                                rosterSizeSourceType: RosterSizeSourceType.Question,
                                children: new IComposite[]
                                {
                                    Create.Entity.SingleOptionQuestion(singleOptionQuestionId, variable: "sgl", linkedToQuestionId: answeredQuestionId),
                                    Create.Entity.Variable(variableId, VariableType.LongInteger, expression: "(long)sgl")
                                }
                            )
                    }));

            interview.AnswerTextListQuestion(userId, answeredQuestionId, RosterVector.Empty, DateTime.Now, new[] { Tuple.Create(1m, "A"), Tuple.Create(2m, "B")});
            interview.AnswerSingleOptionQuestion(userId, singleOptionQuestionId, Create.RosterVector(1), DateTime.Now, 2);

            using (var eventContext = new EventContext())
            {
                interview.AnswerTextListQuestion(userId, answeredQuestionId, RosterVector.Empty, DateTime.Now, new[] { Tuple.Create(1m, "A") });

                return new InvokeResults
                {
                    VariableChangedEventRaised = eventContext.GetSingleEventOrNull<VariablesChanged>()?.ChangedVariables?.Any(x => x.Identity.Id == variableId) ?? false,
                };
            }
        });

        [NUnit.Framework.Test]
        public void should_raise_variables_changd_event_if_related_question_has_answer_removed() 
            => result.VariableChangedEventRaised.Should().BeTrue();

        private static InvokeResults result;

        [Serializable]
        internal class InvokeResults
        {
            public bool VariableChangedEventRaised { get; set; }
        }
    }
}
