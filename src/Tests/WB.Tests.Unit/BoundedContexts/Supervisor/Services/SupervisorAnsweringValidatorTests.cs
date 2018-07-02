using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorAnsweringValidator))]
    public class SupervisorAnsweringValidatorTests
    {
        [Test]
        public void should_not_allow_for_supervisor_to_answer_interviewer_question()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(Id.g1, scope: QuestionScope.Interviewer));

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            var validator = CreateValidator();

            TestDelegate act = () => validator.Validate(interview, Create.Command.AnswerTextQuestionCommand(interview.Id, Id.gA, Id.g1, "answer"));

            Assert.That(act, Throws.Exception.InstanceOf<AnswerNotAcceptedException>());
        }

        private SupervisorAnsweringValidator CreateValidator()
        {
            return new SupervisorAnsweringValidator();
        }
    }
}
