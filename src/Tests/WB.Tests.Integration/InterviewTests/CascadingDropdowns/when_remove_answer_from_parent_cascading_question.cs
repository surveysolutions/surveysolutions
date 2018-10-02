using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_remove_answer_from_parent_cascading_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {

                SetUp.MockedServiceLocator();
                var questionnaireId = Guid.Parse("10000000000000000000000000000000");
                userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
                parentCascadingQuestion = Guid.Parse("22222222222222222222222222222222");
                childCascadingQuestionId = Guid.Parse("21111111111111111111111111111111");
                childOfChildCascadingQuestionId = Guid.Parse("31111111111111111111111111111111");

                QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                    children: new IComposite[]
                    {
                        Create.Entity.SingleQuestion(parentCascadingQuestion, "par",
                            options: new List<Answer> {IntegrationCreate.Answer("1", 1), IntegrationCreate.Answer("2", 2)}),
                        Create.Entity.SingleQuestion(childCascadingQuestionId, "chil",
                            cascadeFromQuestionId: parentCascadingQuestion,
                            options: new List<Answer> {IntegrationCreate.Answer("11", 11, 1), IntegrationCreate.Answer("21", 21, 2)}),
                        Create.Entity.SingleQuestion(childOfChildCascadingQuestionId, "chilchil",
                            cascadeFromQuestionId: childCascadingQuestionId,
                            options: new List<Answer> {IntegrationCreate.Answer("111", 111, 11), IntegrationCreate.Answer("211", 211, 21)})
                    });

                interview = SetupInterview(questionnaire);
                interview.AnswerSingleOptionQuestion(userId, parentCascadingQuestion, RosterVector.Empty, DateTime.Now, 2);
                interview.AnswerSingleOptionQuestion(userId, childCascadingQuestionId, RosterVector.Empty, DateTime.Now, 21);
                interview.AnswerSingleOptionQuestion(userId, childOfChildCascadingQuestionId, RosterVector.Empty, DateTime.Now, 211);

                using (var eventContext = new EventContext())
                {
                    interview.RemoveAnswer(parentCascadingQuestion, RosterVector.Empty, userId, DateTime.Now);
                    return new InvokeResults
                    {
                        WasAnswerOnParentQuestionRemoved = eventContext.AnyEvent<AnswersRemoved>(e=> e.Questions.Count(q => q.Id == parentCascadingQuestion) > 0),
                        WasAnswerOnChildCascadingQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(e=>e.Questions.Count(q => q.Id == childCascadingQuestionId) > 0),
                        WasAnswerOnChildCascadingQuestionRemoved = eventContext.AnyEvent<AnswersRemoved>(e => e.Questions.Count(q => q.Id == childCascadingQuestionId) > 0),
                        WasAnswerOnChildOfChildCascadingQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(e=>e.Questions.Count(q => q.Id == childOfChildCascadingQuestionId) == 1),
                        WasAnswerOnChildOfChildCascadingQuestionRemoved = eventContext.AnyEvent<AnswersRemoved>(e => e.Questions.Count(q => q.Id == childCascadingQuestionId) == 1)
                    };
                }
            });

        [OneTimeTearDown]
        public void TearDown()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [NUnit.Framework.Test] public void should_raise_AnswerRemoved_event_for_parent_question () =>
            result.WasAnswerOnParentQuestionRemoved.Should().BeTrue();

        [NUnit.Framework.Test] public void should_raise_QuestionsDisabled_event_for_child_question () =>
            result.WasAnswerOnChildCascadingQuestionDisabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_raise_QuestionsDisabled_event_for_child_of_child_question () =>
           result.WasAnswerOnChildOfChildCascadingQuestionDisabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_raise_AnswerRemoved_event_for_child_question () =>
            result.WasAnswerOnChildCascadingQuestionRemoved.Should().BeTrue();

        [NUnit.Framework.Test] public void should_raise_AnswerRemoved_event_for_child_of_child_question () =>
            result.WasAnswerOnChildOfChildCascadingQuestionRemoved.Should().BeTrue();

        private static Interview interview;
        private static Guid userId;
        private static Guid parentCascadingQuestion;
        private static Guid childCascadingQuestionId;
        private static Guid childOfChildCascadingQuestionId;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResults result;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasAnswerOnParentQuestionRemoved { get; set; }
            public bool WasAnswerOnChildCascadingQuestionDisabled { get; set; }
            public bool WasAnswerOnChildCascadingQuestionRemoved { get; set; }
            public bool WasAnswerOnChildOfChildCascadingQuestionDisabled { get; set; }
            public bool WasAnswerOnChildOfChildCascadingQuestionRemoved { get; set; }
        }
    }
}
