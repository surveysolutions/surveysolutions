using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.AudioQuestion
{
    [TestFixture]
    internal class AudioQuestionTests : InterviewTestsContext
    {
        [Test]
        public void should_mark_static_text_as_valid_after_answering_audio_question()
        {
            SetUp.MockedServiceLocator();

            var audioId = Guid.Parse("11111111111111111111111111111111");
            var staticId = Guid.Parse("22222222222222222222222222222222");

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.AudioQuestion(audioId, variable: "audio"),
                Create.Entity.StaticText(staticId, validationConditions: new List<ValidationCondition>{ Create.Entity.ValidationCondition("IsAnswered(audio)")} )
            );

            var interview = SetupInterview(questionnaireDocument);

            bool staticTextDeclaredValid;

            using (var eventContext = new EventContext())
            {

                interview.AnswerAudioQuestion(Guid.NewGuid(), audioId, RosterVector.Empty, DateTimeOffset.Now, "some.mp3", new TimeSpan(0, 1, 20));

                var validStaticTexts = eventContext.GetSingleEvent<StaticTextsDeclaredValid>().StaticTexts;

                staticTextDeclaredValid = validStaticTexts.Contains(Create.Identity(staticId, RosterVector.Empty));
            }

            Assert.That(staticTextDeclaredValid, Is.True);
        }

        [Test]
        public void should_mark_static_text_as_invalid_for_created_interview()
        {
            SetUp.MockedServiceLocator();

            var audioId = Guid.Parse("11111111111111111111111111111111");
            var staticId = Guid.Parse("22222222222222222222222222222222");

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.AudioQuestion(audioId, variable: "audio"),
                Create.Entity.StaticText(staticId, validationConditions: new List<ValidationCondition> { Create.Entity.ValidationCondition("IsAnswered(audio)") })
            );

            bool staticTextDeclaredInvalid;

            using (var eventContext = new EventContext())
            {
                var interview = SetupInterview(questionnaireDocument);

                var validStaticTexts = eventContext.GetSingleEvent<StaticTextsDeclaredInvalid>().FailedValidationConditions;

                staticTextDeclaredInvalid = validStaticTexts.Select(x => x.Key).Contains(Create.Identity(staticId, RosterVector.Empty));
            }

            Assert.That(staticTextDeclaredInvalid, Is.True);
        }

    }
}
