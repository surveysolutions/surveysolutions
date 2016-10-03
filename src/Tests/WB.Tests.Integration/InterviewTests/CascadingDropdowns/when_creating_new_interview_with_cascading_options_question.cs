using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_creating_new_interview_with_cascading_options_question : InterviewTestsContext
    {
        private Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();
                var parentSingleOptionQuestionId = Guid.Parse("11111111111111111111111111111111");
                var childCascadedComboboxId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var grandChildCascadedComboboxId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var questionnaireId = Guid.Parse("3B7145CD-A235-44D0-917C-7B34A1017AEC");

                var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                    {
                        Create.Option(value: "1", text: "parent option 1"),
                        Create.Option(value: "2", text: "parent option 2")
                    }),
                    Create.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                        options: new List<Answer>
                        {
                            Create.Option(value: "1", text: "child 1 for parent option 1", parentValue: "1"),
                            Create.Option(value: "2", text: "child 1 for parent option 2", parentValue: "2"),
                        }),
                    Create.SingleQuestion(grandChildCascadedComboboxId, "q3", cascadeFromQuestionId: childCascadedComboboxId,
                        options: new List<Answer>
                        {
                            Create.Option(value: "1", text: "grand child 1 for parent option 1", parentValue: "1"),
                            Create.Option(value: "2", text: "grand child 1 for parent option 2", parentValue: "2"),
                        })
                    );

                using (var eventContext = new EventContext())
                {
                    SetupInterview(questionnaire, new List<object> { });

                    return new InvokeResults
                    {
                        WasChildCascadedComboboxQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasGrandchildCascadedComboboxQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId))
                    };
                }
            });

        It should_disable_first_cascading_question = () =>
           results.WasChildCascadedComboboxQuestionDisabled.ShouldBeTrue();

        It should_disable_secod_level_of_questions_in_cascade = () =>
          results.WasChildCascadedComboboxQuestionDisabled.ShouldBeTrue();

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
            public bool WasChildCascadedComboboxQuestionDisabled { get; set; }
            public bool WasGrandchildCascadedComboboxQuestionDisabled { get; set; }
        }
    }
}