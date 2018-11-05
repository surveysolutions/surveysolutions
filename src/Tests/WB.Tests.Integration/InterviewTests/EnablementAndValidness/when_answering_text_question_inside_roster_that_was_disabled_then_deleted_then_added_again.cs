using System;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_text_question_inside_roster_that_was_disabled_then_deleted_then_added_again : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var idOfQuestionInRoster = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var rosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var rosterSizeQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var integerQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(integerQuestionId, variable: "q0"),
                    Abc.Create.Entity.NumericIntegerQuestion(rosterSizeQuestionId, variable: "q1"),
                    Abc.Create.Entity.Roster(rosterId,
                        rosterSizeQuestionId: rosterSizeQuestionId,
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        enablementCondition: "@rowcode != (decimal)q0",
                        children: new[]
                        {
                            Abc.Create.Entity.Group(null, "Group X", null, "@rowcode != 2", false, new []
                            {
                                Abc.Create.Entity.Question(idOfQuestionInRoster, variable:"q2")
                            })
                                      
                        })
                    );

                var emptyVector = new decimal[] { };
                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new object[] { });

                interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, RosterVector.Empty, DateTime.Now, 3);
                interview.AnswerNumericIntegerQuestion(userId, integerQuestionId, RosterVector.Empty, DateTime.Now, 2);
                interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, RosterVector.Empty, DateTime.Now, 1);
                interview.AnswerNumericIntegerQuestion(userId, integerQuestionId, RosterVector.Empty, DateTime.Now, 8);
                interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, RosterVector.Empty, DateTime.Now, 3);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextQuestion(userId, idOfQuestionInRoster, new decimal[] { 1 }, DateTime.Now, "Hello World!");

                    return new InvokeResults()
                    {
                        WasTextQuestionAnswered = HasEvent<TextQuestionAnswered>(eventContext.Events)
                    };
                }
            });

        [NUnit.Framework.Test] public void should_raise_TextQuestionAnswered_event () =>
            results.WasTextQuestionAnswered.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasTextQuestionAnswered { get; set; }
        }
    }
}
