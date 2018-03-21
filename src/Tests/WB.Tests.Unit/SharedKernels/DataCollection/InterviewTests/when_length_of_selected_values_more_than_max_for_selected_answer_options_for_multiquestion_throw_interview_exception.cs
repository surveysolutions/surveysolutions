using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_length_of_selected_values_more_than_max_for_selected_answer_options_for_multiquestion_throw_interview_exception : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            var sectionId = Guid.Parse("1111111111111111111111111111111A");
            validatingQuestionId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: sectionId, children: new IComposite[]
            {
                Create.Entity.MultipleOptionsQuestion(questionId: validatingQuestionId, answers: new [] { 1, 2, 3, 4 }, maxAllowedAnswers:2),
                
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() => expectedException = NUnit.Framework.Assert.Throws<AnswerNotAcceptedException>(() =>
            interview.AnswerMultipleOptionsQuestion(userId, validatingQuestionId, new decimal[] { }, DateTime.Now, new [] { 1, 2, 3 }));

        [NUnit.Framework.Test] public void should_raise_MultipleOptionsQuestionAnswered_event_with_QuestionId_equal_to_validatingQuestionId () =>
            expectedException.Should().BeOfType(typeof(AnswerNotAcceptedException));

        private static Exception expectedException;
        private static Guid validatingQuestionId;
        private static Interview interview;
        private static Guid userId;
    }
}
