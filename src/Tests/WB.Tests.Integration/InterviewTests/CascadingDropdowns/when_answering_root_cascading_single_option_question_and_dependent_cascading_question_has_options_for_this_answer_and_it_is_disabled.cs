using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_answering_root_cascading_single_option_question_and_dependent_cascading_question_has_options_for_this_answer_and_it_is_disabled : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var parentSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000000");
                var childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");
                var questionnaireId = Guid.Parse("22222222222222222222222222222222");
                var actorId = Guid.Parse("33333333333333333333333333333333");

                SetUp.MockedServiceLocator();

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, Create.Entity.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                {
                    Create.Entity.Option("1", "parent option 1"),
                    Create.Entity.Option("2", "parent option 2")
                }), Create.Entity.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId, options: new List<Answer>
                {
                    Create.Entity.Option("1.1", "child 1 for parent option 1", "1"),
                    Create.Entity.Option("1.2", "child 2 for parent option 1", "1")
                }));

                var interview = SetupInterview(questionnaire, new object[]
                {
                    Create.Event.SingleOptionQuestionAnswered(parentSingleOptionQuestionId, RosterVector.Empty, 2),
                    Create.Event.QuestionsDisabled(new [] { Create.Identity(childCascadedComboboxId) })
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, RosterVector.Empty, DateTime.Now, 1);

                    return new InvokeResults
                    {
                        EnabledQuestions =
                            eventContext.AnyEvent<QuestionsEnabled>()
                                ? eventContext.GetSingleEvent<QuestionsEnabled>().Questions.Select(identity => identity.Id).ToArray()
                                : null
                    };
                }
            });

        [NUnit.Framework.Test] public void should_raise_QuestionsEnabled_event_for_dependent_question () =>
            results.EnabledQuestions.Should().BeEquivalentTo(Guid.Parse("11111111111111111111111111111111"));

        [OneTimeTearDown]
        public void TearDown()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public Guid[] EnabledQuestions { get; set; }
        }
    }
}
