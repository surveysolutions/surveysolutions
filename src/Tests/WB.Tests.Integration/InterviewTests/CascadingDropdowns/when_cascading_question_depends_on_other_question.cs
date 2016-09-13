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
    internal class when_cascading_question_depends_on_other_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {

                Setup.MockedServiceLocator();
                var parentSingleOptionQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var childCascadedComboboxId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var grandChildCascadedComboboxId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var actorId = Guid.Parse("366404A9-374E-4DCC-9B56-1F15103DE880");
                var questionnaireId = Guid.Parse("3B7145CD-A235-44D0-917C-7B34A1017AEC");
                var numericQuestionId = Guid.Parse("11111111-1111-1111-1111-111111111111");

                var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.NumericIntegerQuestion(numericQuestionId, variable: "numeric"),
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
                        enablementCondition: "numeric==2",
                        options: new List<Answer>
                        {
                            Create.Option(value: "1", text: "grand child 1 for parent option 1", parentValue: "1"),
                            Create.Option(value: "2", text: "grand child 1 for parent option 2", parentValue: "2"),
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

        It should_not_enable_grand_child_question = () =>
            results.WasGrandChildEnabled.ShouldBeFalse();

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
            public bool WasGrandChildEnabled { get; set; }
        }
    }
}
