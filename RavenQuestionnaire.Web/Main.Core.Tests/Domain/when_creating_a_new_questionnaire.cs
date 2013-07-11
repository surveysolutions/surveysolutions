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

    /// <summary>
    /// The when_creating_a_new_questionnaire.
    /// </summary>
    [Specification]
    public class when_creating_a_new_questionnaire :
        OneEventTestFixture<CreateQuestionnaireCommand, NewQuestionnaireCreated>
    {
        #region Constants

        /// <summary>
        /// The questionnaire text.
        /// </summary>
        private const string QuestionnaireText = "Questionnaire text goes here";

        #endregion

        #region Fields

        /// <summary>
        /// The now.
        /// </summary>
        private readonly DateTime now = DateTime.UtcNow;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="when_creating_a_new_questionnaire"/> class.
        /// </summary>
        public when_creating_a_new_questionnaire()
        {
            Configuration.Configure();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The the_new_questionnaire_should_have_the_correct_note_id.
        /// </summary>
        [Then]
        public void the_new_questionnaire_should_have_the_correct_note_id()
        {
            Assert.That(this.TheEvent.PublicKey, Is.EqualTo(this.EventSourceId));
        }

        /// <summary>
        /// The the_new_questionnaire_should_have_the_correct_text.
        /// </summary>
        [Then]
        public void the_new_questionnaire_should_have_the_correct_text()
        {
            Assert.That(this.TheEvent.Title, Is.EqualTo(QuestionnaireText));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The given events.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; System.Object].
        /// </returns>
        protected override IEnumerable<object> GivenEvents()
        {
            return new object[0];
        }

        /// <summary>
        /// The register fakes in configuration.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        protected override void RegisterFakesInConfiguration(EnvironmentConfigurationWrapper configuration)
        {
            var clock = new FrozenClock(this.now);
            configuration.Register<IClock>(clock);
        }

        /// <summary>
        /// The when executing.
        /// </summary>
        /// <returns>
        /// The ???.
        /// </returns>
        protected override CreateQuestionnaireCommand WhenExecuting()
        {
            return new CreateQuestionnaireCommand(this.EventSourceId, QuestionnaireText);
        }

        #endregion
    }
}