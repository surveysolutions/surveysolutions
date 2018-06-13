using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_option_that_question_is_linked_to_in_condition_is_removed : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        public void BecauseOf() =>
           results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
           {
               AssemblyContext.SetupServiceLocator();
               Guid userId = Guid.Parse("22222222222222222222222222222222");
               Guid listQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
               Guid linkedSingleOptionQuestion = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
               Guid linkedMultioptionQuestion = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
               Guid questionWithConditionUsingSingleLinked = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
               Guid questionWithConditionUsingMultiLinked = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

               QuestionnaireDocument questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(Guid.Parse("11111111111111111111111111111111"),
                   children: new IComposite[]
                   {
                        Abc.Create.Entity.TextListQuestion(questionId: listQuestionId, variable: "list"),
                        Abc.Create.Entity.SingleOptionQuestion(questionId: linkedSingleOptionQuestion, variable: "lnkSgl", linkedToQuestionId: listQuestionId),
                        Abc.Create.Entity.MultyOptionsQuestion(id: linkedMultioptionQuestion, variable: "lnkMul", linkedToQuestionId: listQuestionId),
                        Abc.Create.Entity.TextQuestion(questionId: questionWithConditionUsingSingleLinked, variable: "txt",
                            enablementCondition: "lnkSgl == 0"),
                        Abc.Create.Entity.TextQuestion(questionId: questionWithConditionUsingMultiLinked, variable: "txt1",
                            enablementCondition: "lnkMul.Contains(0)")
                   });

               var interview = SetupInterview(questionnaireDocument);

               interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.Now, new[] {Tuple.Create(0m, "one")});
               interview.AnswerSingleOptionQuestion(userId, linkedSingleOptionQuestion, RosterVector.Empty, DateTime.Now, 0);
               interview.AnswerMultipleOptionsQuestion(userId, linkedMultioptionQuestion, RosterVector.Empty, DateTime.Now, new [] {0});

               using (var eventContext = new EventContext())
               {
                   interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.Now, new Tuple<decimal, string>[0]);

                   return new InvokeResults
                   {
                        QuestionUsedSingleOptionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(y => y.Id == questionWithConditionUsingSingleLinked)),
                        QuestionUsedMultiOptionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(y => y.Id == questionWithConditionUsingMultiLinked))
                   };
               }
           });

        [NUnit.Framework.Test] public void should_disable_question1 () => results.QuestionUsedSingleOptionDisabled.Should().BeTrue();
        [NUnit.Framework.Test] public void should_disable_question2 () => results.QuestionUsedMultiOptionDisabled.Should().BeTrue();

        static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public bool QuestionUsedSingleOptionDisabled { get; set; }
            public bool QuestionUsedMultiOptionDisabled { get; set; }
        }
    }
}
