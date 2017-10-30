using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Tests.Unit.DataExportTests.ExportedQuestionTests;

namespace WB.Tests.Unit.DataExportTests
{
    [TestOf(typeof(ExportQuestionService))]
    public class ExportQuestionServiceTests : ExportedQuestionTestContext
    {
        [Test]
        public void when_export_text_list_question()
        {
            //arrange
            var interviewTextListAnswers = new[] { new InterviewTextListAnswer(1, "line1"), new InterviewTextListAnswer(2, "line2") };
            
            //act
            var filledQuestion = CreateFilledExportedQuestion(QuestionType.TextList, 3, interviewTextListAnswers);
            var disabledQuestion = CreateDisabledExportedQuestion(QuestionType.TextList, columnsCount: 3);
            var missingQuestion = CreateMissingValueExportedQuestion(QuestionType.TextList, columnsCount: 3);

            //assert
            Assert.That(filledQuestion, Is.EquivalentTo(new[] { "line1", "line2", MissingStringQuestionValue }));
            Assert.That(disabledQuestion, Is.EquivalentTo(new[] { DisableValue, DisableValue, DisableValue }));
            Assert.That(missingQuestion, Is.EquivalentTo(new[] { MissingStringQuestionValue, MissingStringQuestionValue, MissingStringQuestionValue }));
        }

        [Test]
        public void when_export_audio_question()
        {
            //arrange
            var audioAnswer = AudioAnswer.FromString("audio.m4a", TimeSpan.FromSeconds(2));

            //act
            var exportedAudioAnswer = CreateFilledExportedQuestion(QuestionType.Audio, audioAnswer);

            //assert
            Assert.AreEqual(1, exportedAudioAnswer.Length);
            Assert.AreEqual(audioAnswer.FileName, exportedAudioAnswer[0]);
        }
    }
}