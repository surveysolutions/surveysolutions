using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_creating_interview_with_cascading_questions_in_numeric_roster_and_parent_has_condition : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var parentSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000000");
                var childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");
                var grandChildCascadedComboboxId = Guid.Parse("22222222222222222222222222222222");
                var questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var actorId = Guid.Parse("33333333333333333333333333333333");
                var topRosterId = Guid.Parse("44444444444444444444444444444444");
                var numericId = Guid.Parse("55555555555555555555555555555555");

                var questionnaire = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(numericId, variable: "numeric"),
                    Abc.Create.Entity.Roster(topRosterId,
                        variable: "varRoster",
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: numericId,
                        children: new List<IComposite>
                        {
                            Abc.Create.Entity.SingleQuestion(parentSingleOptionQuestionId, "q1", enablementCondition: "numeric > 4",
                                options: new List<Answer>
                                {
                                    Abc.Create.Entity.Option(value: "1", text: "parent option 1"),
                                    Abc.Create.Entity.Option(value: "2", text: "parent option 2")
                                }),
                            Abc.Create.Entity.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                                options:
                                    new List<Answer>
                                    {
                                        Abc.Create.Entity.Option(value: "1", text: "child 1 for parent option 1", parentValue: "1"),
                                        Abc.Create.Entity.Option(value: "3", text: "child 1 for parent option 2", parentValue: "2")
                                    }
                                ),
                            Abc.Create.Entity.SingleQuestion(grandChildCascadedComboboxId, "q3", cascadeFromQuestionId: childCascadedComboboxId,
                                options:
                                    new List<Answer>
                                    {
                                        Abc.Create.Entity.Option(value: "1", text: "child 1 for parent option 1", parentValue: "1"),
                                        Abc.Create.Entity.Option(value: "3", text: "child 1 for parent option 2", parentValue: "2")
                                    }
                                ),

                        })
                    );

                Interview interview = SetupInterview(questionnaire);

                using (var eventContext = new EventContext())
                {

                    interview.AnswerNumericIntegerQuestion(actorId, numericId, new decimal[0], DateTime.Now, 3);
                    return new InvokeResults
                    {
                        WasAnyQuestionEnabled = eventContext.AnyEvent<QuestionsEnabled>(),
                        WasAnyParentQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == parentSingleOptionQuestionId)),
                        WasAnyChildQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasAnyGrandChildQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId)),
                    };
                }
            });

        It should_not_enable_any_question = () =>
            results.WasAnyQuestionEnabled.ShouldBeFalse();

        It should_disable_all_parent_questions = () =>
            results.WasAnyParentQuestionDisabled.ShouldBeTrue();

        It should_disable_all_child_questions = () =>
            results.WasAnyChildQuestionDisabled.ShouldBeTrue();

        It should_disable_all_grand_child_questions = () =>
            results.WasAnyGrandChildQuestionDisabled.ShouldBeTrue();

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
            public bool WasAnyQuestionEnabled { get; set; }
            public bool WasAnyParentQuestionDisabled { get; set; }
            public bool WasAnyChildQuestionDisabled { get; set; }
            public bool WasAnyGrandChildQuestionDisabled { get; set; }
        }
    }
}