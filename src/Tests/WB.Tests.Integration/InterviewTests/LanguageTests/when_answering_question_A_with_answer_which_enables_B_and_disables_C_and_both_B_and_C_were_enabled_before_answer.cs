using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_question_A_with_answer_which_enables_B_and_disables_C_and_both_B_and_C_were_enabled_before_answer : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionA = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                var questionB = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
                var questionC = Guid.Parse("cccccccccccccccccccccccccccccccc");

                var interview = SetupInterview(
                    Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                    {
                        Abc.Create.Entity.Group(null, "Chapter X", null, null, false, new IComposite[]
                        {
                            Abc.Create.Entity.NumericIntegerQuestion(id: questionA, variable: "a"),
                            Abc.Create.Entity.NumericIntegerQuestion(id: questionB, variable: "b", enablementCondition: "a >= 0"),
                            Abc.Create.Entity.NumericIntegerQuestion(id: questionC, variable: "c", enablementCondition: "a <= 0"),
                        }),
                    }),
                    events: new object[]
                    {
                        Abc.Create.Event.NumericIntegerQuestionAnswered(
                            questionA, null, 0, null, null
                        ),
                        Abc.Create.Event.QuestionsEnabled(new[]
                        {
                            Abc.Create.Identity(questionB),
                            Abc.Create.Identity(questionC),
                        }),
                    });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), questionA, RosterVector.Empty, DateTime.Now, 3);

                    return new InvokeResult
                    {
                        QuestionsDisabledEventCount = eventContext.Count<QuestionsDisabled>(),
                        QuestionsEnabledEventCount = eventContext.Count<QuestionsEnabled>(),
                        QuestionsDisabledQuestionIds = eventContext.GetSingleEvent<QuestionsDisabled>().Questions.Select(identity => identity.Id).ToArray(),
                    };
                }
            });

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [NUnit.Framework.Test] public void should_raise_QuestionsDisabled_event () =>
            result.QuestionsDisabledEventCount.Should().Be(1);

        [NUnit.Framework.Test] public void should_not_raise_QuestionsEnabled_event () =>
            result.QuestionsEnabledEventCount.Should().Be(0);

        [NUnit.Framework.Test] public void should_raise_QuestionsDisabled_event_with_question_C_only () =>
            result.QuestionsDisabledQuestionIds.Should().BeEquivalentTo(Guid.Parse("cccccccccccccccccccccccccccccccc"));

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public int QuestionsDisabledEventCount { get; set; }
            public int QuestionsEnabledEventCount { get; set; }
            public Guid[] QuestionsDisabledQuestionIds { get; set; }
        }
    }
}
