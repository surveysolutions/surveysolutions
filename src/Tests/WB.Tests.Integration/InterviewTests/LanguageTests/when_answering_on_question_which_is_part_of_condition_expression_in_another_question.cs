﻿using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_on_question_which_is_part_of_condition_expression_in_another_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {                
                var questionnaireId = Guid.Parse("00000000000000000000000000000000");
                var actorId = Guid.Parse("99999999999999999999999999999999");
                var question1Id = Guid.Parse("11111111111111111111111111111111");
                var question2Id = Guid.Parse("22222222222222222222222222222222");

                Setup.SetupMockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(question1Id, "q1"),
                    Create.NumericIntegerQuestion(question2Id, "q2", "q1 > 3")
               );

                var interview = SetupInterview(questionnaireDocument, new List<object>
                    {
                        new NumericIntegerQuestionAnswered(actorId, question1Id, new decimal[0], DateTime.Now, 1),
                        new NumericIntegerQuestionAnswered(actorId, question1Id, new decimal[0], DateTime.Now, 2),
                        new NumericIntegerQuestionAnswered(actorId, question1Id, new decimal[0], DateTime.Now, 3)
                    });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(actorId, question1Id, new decimal[0], DateTime.Now, 4);

                    result.Questions2Enabled = GetFirstEventByType<QuestionsEnabled>(eventContext.Events).Questions.FirstOrDefault(q => q.Id == question2Id) != null;
                }

                return result;
            });

        It should_enable_second_question = () =>
            results.Questions2Enabled.ShouldBeTrue();

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
            public bool Questions2Enabled { get; set; }
        }
    }
}