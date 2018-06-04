using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Answers
{
    [TestFixture]
    internal class when_answering_audio_question : InterviewAssignTests
    {
        [Test]
        public void should_raise_audio_question_answered_event()
        {
            var audioQuestionid = Id.g1;
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.Question(audioQuestionid, questionType: QuestionType.Audio));

            var interview = CreateInterview(questionnaire);

            using (EventContext eventContext = new EventContext())
            {
                var length = TimeSpan.FromSeconds(54);
                var fileName = "audio.wav";
                interview.AnswerAudioQuestion(Id.g2, audioQuestionid, RosterVector.Empty, DateTimeOffset.Now, fileName, length);

                eventContext.ShouldContainEvent<AudioQuestionAnswered>(x => x.QuestionId == audioQuestionid && x.FileName == fileName && x.Length == length);
            }
        }
    }
}
