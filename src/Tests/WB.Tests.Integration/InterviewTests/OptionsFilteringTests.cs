using System;
using System.Linq;
using AppDomainToolkit;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests
{
    [TestFixture]
    public class OptionsFilteringTests : InterviewTestsContext
    {
        [Test]
        public void when_answer_question_enable_section_with_roster_but_roster_triget_is_disabled()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var numeric1Id = Guid.Parse("11111111111111111111111111111111");
            var numeric2Id = Guid.Parse("22222222222222222222222222222222");
            var singleId = Guid.Parse("33333333333333333333333333333333");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(numeric1Id, variable: "i1"),
                    Create.Entity.NumericIntegerQuestion(numeric2Id, variable: "i2", enablementCondition: "i1 == 1"),
                    Create.Entity.SingleOptionQuestion(singleId, optionsFilterExpression: "@optioncode==1 && i2==1", answerCodes: new decimal[]{ 1, 2, 3})
                );

                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, numeric1Id, RosterVector.Empty, DateTime.UtcNow, 1);
                interview.AnswerNumericIntegerQuestion(userId, numeric2Id, RosterVector.Empty, DateTime.UtcNow, 1);
                interview.AnswerSingleOptionQuestion(userId, singleId, RosterVector.Empty, DateTime.UtcNow, 1);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, numeric1Id, RosterVector.Empty, DateTime.UtcNow, 2);

                    return new
                    {
                        AnswersRemovedEvent = GetFirstEventByType<AnswersRemoved>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.AnswersRemovedEvent, Is.Not.Null);
            Assert.That(results.AnswersRemovedEvent.Questions.Length, Is.EqualTo(1));
            Assert.That(results.AnswersRemovedEvent.Questions.Select(e => e.Id).Contains(singleId), Is.True);


            appDomainContext.Dispose();
            appDomainContext = null;
        }
    }
}
