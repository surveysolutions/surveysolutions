using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    public class when_answering_numeric_question_and_it_is_disabled : InterviewTestsContext
    {
        [Test]
        public void should_throw_interview_exception()
        {
            var questionnaire =
                Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.NumericIntegerQuestion(id: Id.g1));

            var interview = CreateInterview(questionnaire);

            interview.Apply(Create.Event.QuestionsDisabled(new[]{Create.Identity(Id.g1)}));

            TestDelegate act = () => interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: Id.g1));

            Assert.That(act, Throws.Exception.InstanceOf<InterviewException>()
                                   .And.Message.EqualTo("Question (or it's parent) is disabled and question's answer cannot be changed."));
        }
    }
}
