using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Spec;
using Ncqrs.Spec.Fakes;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Events.Questionnaire;

namespace RavenQuestionnaire.Core.Tests.Domain
{
    [Specification]
    public class when_creating_a_new_questionnaire : OneEventTestFixture<CreateQuestionnaireCommand, NewQuestionnaireCreated>
    {

        public when_creating_a_new_questionnaire()
        {
            Configuration.Configure();
        }

        private DateTime now = DateTime.UtcNow;
        private const string QuestionnaireText = "Questionnaire text goes here";

        protected override void RegisterFakesInConfiguration(EnvironmentConfigurationWrapper configuration)
        {
            var clock = new FrozenClock(now);
            configuration.Register<IClock>(clock);
        }

        protected override IEnumerable<object> GivenEvents()
        {
            return new object[0];
        }

        protected override CreateQuestionnaireCommand WhenExecuting()
        {
            return new CreateQuestionnaireCommand(EventSourceId, QuestionnaireText);
        }

        [Then]
        public void the_new_questionnaire_should_have_the_correct_note_id()
        {
            Assert.That(TheEvent.PublicKey, Is.EqualTo(EventSourceId));
        }

        [Then]
        public void the_new_questionnaire_should_have_the_correct_text()
        {
            Assert.That(TheEvent.Title, Is.EqualTo(QuestionnaireText));
        }

       
    }
}
