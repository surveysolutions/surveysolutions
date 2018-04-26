using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    internal partial class ImportDataVerifierTests
    {
        [Test]
        public void when_verify_answers_on_interview_tree_and_questionnaire_has_no_question_Should_report_error()
        {
            var questionnaireDocument = Create.Entity.PlainQuestionnaire();
            var verifier = Create.Service.ImportDataVerifier();

            var answers = new List<InterviewAnswer>
            {
                Create.Entity.InterviewAnswer(Create.Identity(), Create.Entity.TextQuestionAnswer("blabla"))
            };

            var verificationError = verifier.VerifyWithInterviewTree(answers, null, questionnaireDocument);

            Assert.That(verificationError, Is.Not.Null);
        }
    }
}
