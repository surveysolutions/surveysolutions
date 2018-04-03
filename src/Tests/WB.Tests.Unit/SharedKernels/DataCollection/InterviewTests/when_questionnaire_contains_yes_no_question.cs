using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_questionnaire_contains_yes_no_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
            {
                Create.Entity.YesNoQuestion(questionId: questionId, answers: new[]
                {
                    11,
                    222,
                    3333,
                }),
            });

            interview = Setup.InterviewForQuestionnaireDocument(questionnaireDocument);
            BecauseOf();
        }

        public void BecauseOf() => 
            coughtException = 
              NUnit.Framework.Assert.Throws<InterviewException>(() => interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), questionId, RosterVector.Empty, DateTime.Now, new[] { 11}));

        [NUnit.Framework.Test] public void should_not_allow_to_answer_on_yes_no_question_using_multiopions_question_method () => 
            coughtException.Message.ToLower().ToSeparateWords().Should().Contain("yes/no", "multiple", "options");

        static Interview interview;
        static Guid questionId;
        static InterviewException coughtException;
    }
}
