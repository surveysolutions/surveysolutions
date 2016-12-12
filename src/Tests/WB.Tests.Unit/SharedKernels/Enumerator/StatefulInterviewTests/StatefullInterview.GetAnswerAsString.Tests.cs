using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [TestOf(typeof(StatefulInterview))]
    internal partial class StatefullInterviewTests
    {
        [Test]
        public void When_GetAnswerAsString_for_prefilled_single_fixed_question_Then_should_be_returned_answered_option_text()
        {
            //arrange
            var singleFilxedQuestionIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"),
                RosterVector.Empty);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(singleFilxedQuestionIdentity.Id, isPrefilled: true,
                    answers: new List<Answer> {Create.Entity.Answer("one", 1), Create.Entity.Answer("two", 2)}));
            var interviewerId = Guid.Parse("22222222222222222222222222222222");
            var interview = Create.AggregateRoot.StatefulInterview(userId: interviewerId, questionnaire: Create.Entity.PlainQuestionnaire(questionnaire));
            interview.AnswerSingleOptionQuestion(interviewerId, singleFilxedQuestionIdentity.Id, singleFilxedQuestionIdentity.RosterVector, DateTime.UtcNow, 2);
            //act
            var stringAnswerOnPrefilledSingleQuestion = interview.GetAnswerAsString(singleFilxedQuestionIdentity);
            //assert
            Assert.That(stringAnswerOnPrefilledSingleQuestion, Is.EqualTo("two"));
        }
    }

}