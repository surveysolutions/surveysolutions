﻿using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_text_question_A_and_that_answer_disables_question_B_and_disabled_B_disables_question_C_and_B_was_answered : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var emptyRosterVector = new decimal[] { };
                var userId = Guid.Parse("11111111111111111111111111111111");
                var questionnaireId = Guid.Parse("22222222222222222222222222222222");
                var answerTime = new DateTime(2014, 1, 1);

                var questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var questionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                Setup.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.Question(questionAId, "a"),
                    Abc.Create.Entity.Question(questionBId, "b", enablementCondition: "a == 1.ToString()"),
                    Abc.Create.Entity.Question(questionCId, "c", enablementCondition: "b == 1.ToString()")
                );

                var interview = SetupInterviewWithProcessor(questionnaireDocument, new List<object>
                {
                    Abc.Create.Event.QuestionsEnabled(new []
                    {
                        Abc.Create.Identity(questionAId),
                        Abc.Create.Identity(questionBId),
                        Abc.Create.Identity(questionCId)
                    }),
                    Abc.Create.Event.TextQuestionAnswered(questionBId, null, "2", null, null)
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextQuestion(userId, questionAId, emptyRosterVector, answerTime, "disable B please");

                    return new InvokeResults
                    {
                        QuestionBDisabled = GetFirstEventByType<QuestionsDisabled>(eventContext.Events).Questions.First(q => q.Id == questionBId) != null,
                        QuestionCDisabled = GetFirstEventByType<QuestionsDisabled>(eventContext.Events).Questions.First(q => q.Id == questionCId) != null,
                    };
                }
            });

        It should_disable_question_B = () =>
            results.QuestionBDisabled.ShouldBeTrue();

        It should_disable_question_C = () =>
            results.QuestionCDisabled.ShouldBeTrue();

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
            public bool QuestionBDisabled { get; set; }
            public bool QuestionCDisabled { get; set; }
        }
    }
}