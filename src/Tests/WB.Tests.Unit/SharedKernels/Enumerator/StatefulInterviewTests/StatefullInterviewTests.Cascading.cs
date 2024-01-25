using System;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal partial class StatefulInterviewTests
    {
        [Test]
        public void When_cascading_question_has_less_option_than_threshold()
        {
            //arrange

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(questionId: Id.g1, answerCodes: new[] { 1, 2 }, variable: "q1"),
                Create.Entity.SingleOptionQuestion(questionId: Id.g2, cascadeFromQuestionId: Id.g1, answerCodes: new[] { 1, 2, 3, 4 }, parentCodes: new[] { 1, 1, 2, 2 }, variable: "q2", showAsListThreshold: 3));

            var options = new[]
            {
                Create.Entity.CategoricalQuestionOption(3, title: "3", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(4, title: "4", parentValue: 2)
            };
            var optionsRepository = Mock.Of<IQuestionOptionsRepository>(x => x.GetOptionsForQuestion(It.IsAny<IQuestionnaire>(), Id.g2, 2, null, null, null) == options);

            var interview = Create.AggregateRoot.StatefulInterview(userId: Id.gA, questionnaire: questionnaire, optionsRepository: optionsRepository);
            interview.AnswerSingleOptionQuestion(Id.gA, Id.g1, RosterVector.Empty, DateTime.UtcNow, 2);

            //act
            var hasMoreOptionsThenInThreshold = interview.DoesCascadingQuestionHaveMoreOptionsThanThreshold(Create.Identity(Id.g2, RosterVector.Empty), threshold: 3);

            //assert
            Assert.That(hasMoreOptionsThenInThreshold, Is.False);
        }

        [Test]
        public void When_cascading_question_has_more_option_than_threshold()
        {
            //arrange
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(questionId: Id.g1, answerCodes: new[] { 1, 2 }, variable: "q1"),
                Create.Entity.SingleOptionQuestion(questionId: Id.g2, cascadeFromQuestionId: Id.g1, answerCodes: new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, parentCodes: new[] { 1, 1, 2, 2, 2, 2, 2, 2, 2 }, variable: "q2", showAsListThreshold: 5));
            
            var options = new[]
            {
                Create.Entity.CategoricalQuestionOption(3, title: "3", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(4, title: "4", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(5, title: "5", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(6, title: "6", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(7, title: "7", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(8, title: "8", parentValue: 2),
                Create.Entity.CategoricalQuestionOption(9, title: "9", parentValue: 2)
            };
            var optionsRepository = Mock.Of<IQuestionOptionsRepository>(x => x.GetOptionsForQuestion(It.IsAny<IQuestionnaire>(), Id.g2, 2, null, null, null) == options);

            var interview = Create.AggregateRoot.StatefulInterview(userId: Id.gA, questionnaire: questionnaire, optionsRepository: optionsRepository);
            interview.AnswerSingleOptionQuestion(Id.gA, Id.g1, RosterVector.Empty, DateTime.UtcNow, 2);

            //act
            var hasMoreOptionsThenInThreshold = interview.DoesCascadingQuestionHaveMoreOptionsThanThreshold(Create.Identity(Id.g2, RosterVector.Empty), threshold: 5);

            //assert
            Assert.That(hasMoreOptionsThenInThreshold, Is.True);
        }
    }
}
