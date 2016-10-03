using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_remove_answer_from_parent_cascading_question : InterviewTestsContext
    {
        private Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();

        };

        private Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {

                Setup.MockedServiceLocator();
                var questionnaireId = Guid.Parse("10000000000000000000000000000000");
                userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
                parentCascadingQuestion = Guid.Parse("22222222222222222222222222222222");
                childCascadingQuestionId = Guid.Parse("21111111111111111111111111111111");
                childOfChildCascadingQuestionId = Guid.Parse("31111111111111111111111111111111");

                QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                    children: new IComposite[]
                    {
                        Create.SingleQuestion(id: parentCascadingQuestion, variable: "par",
                            options: new List<Answer>() {Create.Answer("1", 1), Create.Answer("2", 2)}),
                        Create.SingleQuestion(id: childCascadingQuestionId, variable: "chil",
                            cascadeFromQuestionId: parentCascadingQuestion,
                            options: new List<Answer>() {Create.Answer("11", 11, 1), Create.Answer("21", 21, 2)}),
                        Create.SingleQuestion(id: childOfChildCascadingQuestionId, variable: "chilchil",
                            cascadeFromQuestionId: childCascadingQuestionId,
                            options: new List<Answer>() {Create.Answer("111", 111, 11), Create.Answer("211", 211, 21)})
                    });

                interview = SetupInterview(questionnaire);
                interview.AnswerSingleOptionQuestion(userId, parentCascadingQuestion, new decimal[0],
                    DateTime.Now, 2);

                interview.AnswerSingleOptionQuestion(userId, childCascadingQuestionId, new decimal[0],
                    DateTime.Now, 21);

                interview.AnswerSingleOptionQuestion(userId, childOfChildCascadingQuestionId, new decimal[0],
                 DateTime.Now, 211);
                using (var eventContext = new EventContext())
                {

                    interview.RemoveAnswer(parentCascadingQuestion, new decimal[0], userId, DateTime.Now);
                    return new InvokeResults()
                    {
                        WasAnswerOnParentQuestionRemoved = eventContext.AnyEvent<AnswerRemoved>(e=> e.QuestionId == parentCascadingQuestion && !e.RosterVector.Any()),
                        WasAnswerOnChildCascadingQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(e=>e.Questions.Count(q => q.Id == childCascadingQuestionId && !q.RosterVector.Any()) > 0),
                        WasAnswerOnChildCascadingQuestionRemoved = eventContext.AnyEvent<AnswersRemoved>(e => e.Questions.Count(q => q.Id == childCascadingQuestionId && !q.RosterVector.Any()) > 0),
                        WasAnswerOnChildOfChildCascadingQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(e=>e.Questions.Count(q => q.Id == childOfChildCascadingQuestionId && !q.RosterVector.Any()) == 1),
                        WasAnswerOnChildOfChildCascadingQuestionRemoved = eventContext.AnyEvent<AnswersRemoved>(e => e.Questions.Count(q => q.Id == childCascadingQuestionId && !q.RosterVector.Any()) == 1)
                    };
                }
            });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_raise_AnswerRemoved_event_for_parent_question = () =>
            result.WasAnswerOnParentQuestionRemoved.ShouldBeTrue();

        It should_raise_QuestionsDisabled_event_for_child_question = () =>
            result.WasAnswerOnChildCascadingQuestionDisabled.ShouldBeTrue();

        It should_raise_QuestionsDisabled_event_for_child_of_child_question = () =>
           result.WasAnswerOnChildOfChildCascadingQuestionDisabled.ShouldBeTrue();

        It should_raise_AnswerRemoved_event_for_child_question = () =>
            result.WasAnswerOnChildCascadingQuestionRemoved.ShouldBeTrue();

        It should_raise_AnswerRemoved_event_for_child_of_child_question = () =>
            result.WasAnswerOnChildOfChildCascadingQuestionRemoved.ShouldBeTrue();

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