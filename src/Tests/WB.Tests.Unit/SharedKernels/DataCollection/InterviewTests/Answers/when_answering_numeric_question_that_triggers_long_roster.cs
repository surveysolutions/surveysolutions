using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Answers
{
    [TestOf(typeof(Interview))]
    internal class when_answering_numeric_question_that_triggers_long_roster : InterviewTestsContext
    {
        [Test]
        public void should_throw_exception_when_answering_more_than_allowed_value_for_long_rosters()
        {
            var rosterTriggerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var maxRosterNodesCount = Constants.MaxAmountOfItemsInLongRoster;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                null,
                questionnaireId,
                Create.Entity.NumericIntegerQuestion(
                    id: rosterTriggerId,
                    scope: QuestionScope.Headquarter),
                Create.Entity.Roster(rosterId: rosterId,
                    rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: rosterTriggerId,
                    children: Enumerable.Range(0, maxRosterNodesCount / 2)
                                        .Select(index => Create.Entity.TextQuestion(variable: "text_question_" + index))
                                        .Concat(((IComposite)Create.Entity.Group(
                                            children: Enumerable.Range(0, maxRosterNodesCount / 2)
                                                            .Select(index => Create.Entity.TextQuestion(variable: "sub_text_question_" + index)))).ToEnumerable())));

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                plainQuestionnaire);

            var interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            var exception = Assert.Catch<AnswerNotAcceptedException>(() =>
            {
                interview.AnswerNumericIntegerQuestion(Guid.NewGuid(),
                    rosterTriggerId,
                    RosterVector.Empty,
                    DateTime.Now,
                    plainQuestionnaire.GetMaxRosterRowCount() + 1);
            });

            Assert.That(exception, Is.Not.Null, $"It should throw {typeof(AnswerNotAcceptedException)}");
        }
    }
}