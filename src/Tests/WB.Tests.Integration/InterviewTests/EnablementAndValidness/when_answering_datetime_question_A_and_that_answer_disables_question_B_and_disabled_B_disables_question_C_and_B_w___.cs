using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Tests.Integration.InterviewTests.LanguageTests;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_datetime_question_A_and_that_answer_disables_question_B_and_disabled_B_disables_question_C_and_B_was_answered : CodeGenerationTestsContext
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

                Setup.SetupMockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.DateTimeQuestion(questionAId, "a"),
                    Create.DateTimeQuestion(questionBId, "b", "a > new DateTime(2015, 2, 2)"),
                    Create.DateTimeQuestion(questionCId, "c", "b > new DateTime(2015, 9, 9 )")
                );

                var interview = SetupInterview(questionnaireDocument, new List<object>
                {
                    new QuestionsEnabled(new[]{ new Identity(questionAId, emptyRosterVector), new Identity(questionBId, emptyRosterVector), new Identity(questionCId, emptyRosterVector) }),
                    new DateTimeQuestionAnswered(userId, questionBId, emptyRosterVector, answerTime, new DateTime(2012, 1, 1))
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerDateTimeQuestion(userId, questionAId, emptyRosterVector, answerTime, new DateTime(2014, 10, 10));

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
        private static AppDomainContext appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionBDisabled { get; set; }
            public bool QuestionCDisabled { get; set; }
        }
    }
}