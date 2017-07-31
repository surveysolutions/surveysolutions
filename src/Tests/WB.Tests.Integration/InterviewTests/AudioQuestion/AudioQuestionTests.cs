using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
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
        [SetUp]
        public void Setup()
        {
            appDomainContext = AppDomainContext.Create();
        }

        [TearDown]
        public void TearDown()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void should_mark_static_text_as_valid_after_answering_audio_question()
        {
            var staticTextDeclaredValid = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Integration.Setup.MockedServiceLocator();

                var audioId = Guid.Parse("11111111111111111111111111111111");
                var staticId = Guid.Parse("22222222222222222222222222222222");

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.AudioQuestion(audioId, variable: "audio"),
                    Create.Entity.StaticText(staticId, validationConditions: new List<ValidationCondition>{ Create.Entity.ValidationCondition("IsAnswered(audio)")} )
                );

                var interview = SetupInterview(questionnaireDocument);

                using (var eventContext = new EventContext())
                {

                    interview.AnswerAudioQuestion(Guid.NewGuid(), audioId, RosterVector.Empty, DateTime.Now, "some.mp3",
                        new TimeSpan(0, 1, 20));

                    var validStaticTexts = eventContext.GetSingleEvent<StaticTextsDeclaredValid>().StaticTexts;

                    return validStaticTexts.Contains(Create.Identity(staticId, RosterVector.Empty));
                }
            });

            Assert.That(staticTextDeclaredValid, Is.True);
        }

        [Test]
        public void should_mark_static_text_as_invalid_for_created_interview()
        {
            var staticTextDeclaredInvalid = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Integration.Setup.MockedServiceLocator();

                var audioId = Guid.Parse("11111111111111111111111111111111");
                var staticId = Guid.Parse("22222222222222222222222222222222");

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.AudioQuestion(audioId, variable: "audio"),
                    Create.Entity.StaticText(staticId, validationConditions: new List<ValidationCondition> { Create.Entity.ValidationCondition("IsAnswered(audio)") })
                );

                using (var eventContext = new EventContext())
                {
                    var interview = SetupInterview(questionnaireDocument);

                    var validStaticTexts = eventContext.GetSingleEvent<StaticTextsDeclaredInvalid>().FailedValidationConditions;

                    return validStaticTexts.Select(x => x.Key).Contains(Create.Identity(staticId, RosterVector.Empty));
                }
            });

            Assert.That(staticTextDeclaredInvalid, Is.True);
        }

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
    }
}
