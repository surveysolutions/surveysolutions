using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_cascading_question_depends_on_other_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {

                SetUp.MockedServiceLocator();
                var parentSingleOptionQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var childCascadedComboboxId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var grandChildCascadedComboboxId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var actorId = Guid.Parse("366404A9-374E-4DCC-9B56-1F15103DE880");
                var questionnaireId = Guid.Parse("3B7145CD-A235-44D0-917C-7B34A1017AEC");
                var numericQuestionId = Guid.Parse("11111111-1111-1111-1111-111111111111");

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(numericQuestionId, "numeric"),
                    Create.Entity.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                    {
                        Create.Entity.Option("1", "parent option 1"),
                        Create.Entity.Option("2", "parent option 2")
                    }),
                    Create.Entity.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                        options: new List<Answer>
                        {
                            Create.Entity.Option("1", "child 1 for parent option 1", "1"),
                            Create.Entity.Option("2", "child 1 for parent option 2", "2")
                        }),
                    Create.Entity.SingleQuestion(grandChildCascadedComboboxId, "q3", cascadeFromQuestionId: childCascadedComboboxId,
                        enablementCondition: "numeric==2",
                        options: new List<Answer>
                        {
                            Create.Entity.Option("1", "grand child 1 for parent option 1", "1"),
                            Create.Entity.Option("2", "grand child 1 for parent option 2", "2")
                        })
                    );

                var interview = SetupInterview(questionnaire);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(actorId, numericQuestionId, new decimal[] { }, DateTime.Now, 2);

                    return new InvokeResults
                    {
                        WasGrandChildEnabled = eventContext.AnyEvent<QuestionsEnabled>(e => e.Questions.Any(q => q.Id == grandChildCascadedComboboxId))
                    };
                }
            });

        [NUnit.Framework.Test] public void should_not_enable_grand_child_question () =>
            results.WasGrandChildEnabled.Should().BeFalse();

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
            public bool WasGrandChildEnabled { get; set; }
        }
    }
}
