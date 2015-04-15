using System;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_text_question_inside_roster_that_was_disabled_then_deleted_then_added_again : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var idOfQuestionInRoster = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var rosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var rosterSizeQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var integerQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(integerQuestionId, variable: "q0"),
                    Create.NumericIntegerQuestion(rosterSizeQuestionId, variable: "q1"),
                    Create.Roster(rosterId,
                        rosterSizeQuestionId: rosterSizeQuestionId,
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        enablementCondition: "@rowcode != (decimal)q0",
                        children: new[]
                                  {
                                      Create.Group(
                                          enablementCondition: "@rowcode != 2",
                                          children: new []
                                                    {
                                                        Create.Question(idOfQuestionInRoster, variable:"q2")
                                                    })
                                      
                                  })
                    );

                var emptyVector = new decimal[] { };
                var interview = SetupInterview(questionnaireDocument, new object[] { });

                interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, Empty.RosterVector, DateTime.Now, 3);
                interview.AnswerNumericIntegerQuestion(userId, integerQuestionId, Empty.RosterVector, DateTime.Now, 2);
                interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, Empty.RosterVector, DateTime.Now, 1);
                interview.AnswerNumericIntegerQuestion(userId, integerQuestionId, Empty.RosterVector, DateTime.Now, 8);
                interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, Empty.RosterVector, DateTime.Now, 3);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextQuestion(userId, idOfQuestionInRoster, new decimal[] { 1 }, DateTime.Now, "Hello World!");

                    return new InvokeResults()
                           {
                               WasTextQuestionAnswered = HasEvent<TextQuestionAnswered>(eventContext.Events)
                           };
                }
            });

        It should_raise_TextQuestionAnswered_event = () =>
            results.WasTextQuestionAnswered.ShouldBeTrue();

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
            public bool WasTextQuestionAnswered { get; set; }
        }
    }
}