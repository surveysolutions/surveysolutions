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
            var interviewTextListAnswers = new[] { new InterviewTextListAnswer(1, "List 1"), new InterviewTextListAnswer(2, "List 2") };

            //act
            var textListAnswers = CreateFilledExportedQuestion(QuestionType.TextList, interviewTextListAnswers,
                columnNames: new[] {"List comumn 1", "List column 2"});

            //assert
            Assert.AreEqual(2, textListAnswers.Length);
            Assert.That(textListAnswers, Is.EquivalentTo(interviewTextListAnswers.Select(x => x.Answer)));
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