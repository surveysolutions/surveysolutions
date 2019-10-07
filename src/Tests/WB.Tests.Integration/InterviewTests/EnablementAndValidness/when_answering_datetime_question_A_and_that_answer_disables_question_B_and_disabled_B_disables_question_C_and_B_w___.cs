using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_datetime_question_A_and_that_answer_disables_question_B_and_disabled_B_disables_question_C_and_B_was_answered : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var emptyRosterVector = new decimal[] { };
                var userId = Guid.Parse("11111111111111111111111111111111");
                var questionnaireId = Guid.Parse("22222222222222222222222222222222");
                var answerTime = new DateTime(2014, 1, 1);

                var questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var questionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                     Abc.Create.Entity.DateTimeQuestion(questionAId, variable: "a"),
                     Abc.Create.Entity.DateTimeQuestion(questionBId, variable: "b", enablementCondition: "a > new DateTime(2015, 2, 2)"),
                     Abc.Create.Entity.DateTimeQuestion(questionCId, variable: "c", enablementCondition: "b > new DateTime(2015, 9, 9 )")
                );

                var interview = SetupInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument, new List<object>
                {
                    Abc.Create.Event.QuestionsEnabled(new []
                    {
                        Abc.Create.Identity(questionAId),
                        Abc.Create.Identity(questionBId),
                        Abc.Create.Identity(questionCId)
                    }),
                    Abc.Create.Event.DateTimeQuestionAnswered(questionBId, new DateTime(2012, 1, 1), null)
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

        [NUnit.Framework.Test] public void should_disable_question_B () =>
            results.QuestionBDisabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_disable_question_C () =>
            results.QuestionCDisabled.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

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
