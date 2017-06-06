using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewSynchronizationDtoFactoryTests
{
    [TestOf(typeof(InterviewSynchronizationDtoFactory))]
    [TestFixture]
    public class InterviewSynchronizationDtoFactoryTests
    {
        [Test]
        public void when_build_from_interview_with_readonly_question_should_result_has_this_question_in_readonly_collection()
        {
            //arrange
            Guid questionId = Guid.Parse("21111111111111111111111111111111");
            InterviewData interviewData = Create.Entity.InterviewData(new InterviewQuestion[]
            {
                new InterviewQuestion(questionId) { QuestionState = QuestionState.Readonly }, 
            });

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                new NumericQuestion("n1")
                {
                    QuestionType = QuestionType.Numeric,
                    PublicKey = questionId,
                });

            var interviewSynchronizationDtoFactory = Setup.InterviewSynchronizationDtoFactory(questionnaireDocument);

            //act
            var result = interviewSynchronizationDtoFactory.BuildFrom(interviewData, "comment", null, null);

            //assert
            Assert.That(result.ReadonlyQuestions.Count, Is.EqualTo(1));
            Assert.That(result.ReadonlyQuestions.Single().Id, Is.EqualTo(questionId));
        }
    }
}