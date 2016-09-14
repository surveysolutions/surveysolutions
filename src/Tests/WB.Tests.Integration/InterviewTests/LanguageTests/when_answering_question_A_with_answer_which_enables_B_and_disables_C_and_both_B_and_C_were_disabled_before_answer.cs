using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_question_A_with_answer_which_enables_B_and_disables_C_and_both_B_and_C_were_disabled_before_answer : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionA = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                var questionB = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
                var questionC = Guid.Parse("cccccccccccccccccccccccccccccccc");

                var interview = SetupInterview(
                    Create.QuestionnaireDocumentWithOneChapter(children: new[]
                    {
                        Create.Chapter(children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(id: questionA, variable: "a"),
                            Create.NumericIntegerQuestion(id: questionB, variable: "b", enablementCondition: "a > 0"),
                            Create.NumericIntegerQuestion(id: questionC, variable: "c", enablementCondition: "a < 0"),
                        }),
                    }),
                    events: new object[]
                    {
                        Create.Event.NumericIntegerQuestionAnswered(questionId: questionA, answer: 0),
                        Create.Event.QuestionsDisabled(new[]
                        {
                            Create.Identity(questionB),
                            Create.Identity(questionC),
                        }),
                    });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), questionA, Empty.RosterVector, DateTime.Now, 3);

                    return new InvokeResult
                    {
                        QuestionsDisabledEventCount = eventContext.Count<QuestionsDisabled>(),
                        QuestionsEnabledEventCount = eventContext.Count<QuestionsEnabled>(),
                        QuestionsEnabledQuestionIds = eventContext.GetSingleEvent<QuestionsEnabled>().Questions.Select(identity => identity.Id).ToArray(),
                    };
                }
            });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_not_raise_QuestionsDisabled_event = () =>
            result.QuestionsDisabledEventCount.ShouldEqual(0);

        It should_raise_QuestionsEnabled_event = () =>
            result.QuestionsEnabledEventCount.ShouldEqual(1);

        It should_raise_QuestionsEnabled_event_with_question_B_only = () =>
            result.QuestionsEnabledQuestionIds.ShouldContainOnly(Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"));

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public int QuestionsDisabledEventCount { get; set; }
            public int QuestionsEnabledEventCount { get; set; }
            public Guid[] QuestionsEnabledQuestionIds { get; set; }
        }
    }
}