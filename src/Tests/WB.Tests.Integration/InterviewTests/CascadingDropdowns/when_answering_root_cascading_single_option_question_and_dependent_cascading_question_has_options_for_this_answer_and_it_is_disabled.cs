using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_answering_root_cascading_single_option_question_and_dependent_cascading_question_has_options_for_this_answer_and_it_is_disabled : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var parentSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000000");
                var childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");
                var questionnaireId = Guid.Parse("22222222222222222222222222222222");
                var actorId = Guid.Parse("33333333333333333333333333333333");

                Setup.MockedServiceLocator();

                var questionnaire = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, new[]
                {
                    Abc.Create.Entity.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                    {
                        Abc.Create.Entity.Option(value: "1", text: "parent option 1"),
                        Abc.Create.Entity.Option(value: "2", text: "parent option 2")
                    }),
                    Abc.Create.Entity.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId, options: new List<Answer>
                    {
                        Abc.Create.Entity.Option(value: "1.1", text: "child 1 for parent option 1", parentValue: "1"),
                        Abc.Create.Entity.Option(value: "1.2", text: "child 2 for parent option 1", parentValue: "1"),
                    }),
                });

                var interview = SetupInterview(questionnaire, new object[]
                {
                    Abc.Create.Event.SingleOptionQuestionAnswered(parentSingleOptionQuestionId, RosterVector.Empty, 2),
                    Abc.Create.Event.QuestionsDisabled(new [] { IntegrationCreate.Identity(childCascadedComboboxId) }),
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, RosterVector.Empty, DateTime.Now, 1m);

                    return new InvokeResults
                    {
                        EnabledQuestions =
                            eventContext.AnyEvent<QuestionsEnabled>()
                                ? eventContext.GetSingleEvent<QuestionsEnabled>().Questions.Select(identity => identity.Id).ToArray()
                                : null,
                    };
                }
            });

        It should_raise_QuestionsEnabled_event_for_dependent_question = () =>
            results.EnabledQuestions.ShouldContainOnly(Guid.Parse("11111111111111111111111111111111"));

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
            public Guid[] EnabledQuestions { get; set; }
        }
    }
}