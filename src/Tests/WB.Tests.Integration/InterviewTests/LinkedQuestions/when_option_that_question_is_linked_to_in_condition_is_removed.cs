﻿using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_option_that_question_is_linked_to_in_condition_is_removed : in_standalone_app_domain
    {
        Because of = () =>
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

        It should_disable_question1 = () => results.QuestionUsedSingleOptionDisabled.ShouldBeTrue();
        It should_disable_question2 = () => results.QuestionUsedMultiOptionDisabled.ShouldBeTrue();

        static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public bool QuestionUsedSingleOptionDisabled { get; set; }
            public bool QuestionUsedMultiOptionDisabled { get; set; }
        }
    }
}