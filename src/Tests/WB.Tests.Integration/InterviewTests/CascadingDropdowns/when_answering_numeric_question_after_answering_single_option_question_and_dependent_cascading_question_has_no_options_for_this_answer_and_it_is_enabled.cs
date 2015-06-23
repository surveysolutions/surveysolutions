using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_answering_numeric_question_after_answering_single_option_question_and_dependent_cascading_question_has_no_options_for_this_answer_and_it_is_enabled : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var parentSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000000");
                var questionnaireId = Guid.Parse("22222222222222222222222222222222");
                var actorId = Guid.Parse("33333333333333333333333333333333");
                var numericId = Guid.Parse("44444444444444444444444444444444");

                Setup.MockedServiceLocator();

                var questionnaire = Create.QuestionnaireDocument(questionnaireId, new AbstractQuestion[]
                {
                    Create.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                    {
                        Create.Option(text: "parent option 1", value: "1"),
                        Create.Option(text: "parent option 2", value: "2")
                    }),
                    Create.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId, options: new List<Answer>
                    {
                        Create.Option(text: "child 1 for parent option 1", value: "1.1", parentValue: "1"),
                        Create.Option(text: "child 2 for parent option 1", value: "1.2", parentValue: "1"),
                    }),
                    Create.NumericIntegerQuestion(numericId, "numeric")
                });

                var interview = SetupInterview(questionnaire, new object[]{});

                interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 1m);
                interview.AnswerSingleOptionQuestion(actorId, childCascadedComboboxId, new decimal[] { }, DateTime.Now, 1.1m);
                interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 2m);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(actorId, numericId, new decimal[] { }, DateTime.Now, 2);

                    return new InvokeResults
                           {
                               DisabledQuestions =
                                   eventContext.AnyEvent<QuestionsDisabled>()
                                       ? eventContext.GetSingleEvent<QuestionsDisabled>().Questions.Select(identity => identity.Id).ToArray()
                                       : null,
                               EnabledQuestions =
                                    eventContext.AnyEvent<QuestionsEnabled>()
                                       ? eventContext.GetSingleEvent<QuestionsEnabled>().Questions.Select(identity => identity.Id).ToArray()
                                       : null,
                           };
                }
            });

        It should_not_raise_QuestionsDisabled_event = () =>
            results.DisabledQuestions.ShouldBeNull();

        It should_not_raise_QuestionsEnabled_event = () =>
            results.EnabledQuestions.ShouldBeNull();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext appDomainContext;

        private static readonly Guid childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");

        [Serializable]
        internal class InvokeResults
        {
            public Guid[] DisabledQuestions { get; set; }
            public Guid[] EnabledQuestions { get; set; }
        }
    }
}