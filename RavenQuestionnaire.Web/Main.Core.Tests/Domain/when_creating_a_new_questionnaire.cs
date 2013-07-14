namespace Main.Core.Tests.Domain
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Commands.Questionnaire;
    using Main.Core.Events.Questionnaire;

    using Ncqrs;
    using Ncqrs.Spec;
    using Ncqrs.Spec.Fakes;

    using NUnit.Framework;

    [Specification]
    public class when_creating_a_new_questionnaire :
        OneEventTestFixture<CreateQuestionnaireCommand, NewQuestionnaireCreated>
    {
        private const string QuestionnaireText = "Questionnaire text goes here";
        private readonly DateTime now = DateTime.UtcNow;

        public when_creating_a_new_questionnaire()
        {
            Configuration.Configure();
        }

        [Then]
        public void the_new_questionnaire_should_have_the_correct_note_id()
        {
            Assert.That(this.TheEvent.PublicKey, Is.EqualTo(this.EventSourceId));
        }

        [Then]
        public void the_new_questionnaire_should_have_the_correct_text()
        {
            Assert.That(this.TheEvent.Title, Is.EqualTo(QuestionnaireText));
        }

        protected override IEnumerable<object> GivenEvents()
        {
            return new object[0];
        }

        protected override void RegisterFakesInConfiguration(EnvironmentConfigurationWrapper configuration)
        {
            var clock = new FrozenClock(this.now);
            configuration.Register<IClock>(clock);
        }

        protected override CreateQuestionnaireCommand WhenExecuting()
        {
            return new CreateQuestionnaireCommand(this.EventSourceId, QuestionnaireText);
        }
    }
}