namespace Main.Core.Tests.Domain
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Commands.Questionnaire;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.Questionnaire;

    using Ncqrs;
    using Ncqrs.Spec;
    using Ncqrs.Spec.Fakes;

    using NUnit.Framework;

    /// <summary>
    /// The when_updating_image.
    /// </summary>
    [Specification]
    public class when_updating_image : OneEventTestFixture<UpdateImageCommand, ImageUpdated>
    {
        #region Constants

        /// <summary>
        /// The image description.
        /// </summary>
        private const string ImageDescription = "Image description goes here";

        /// <summary>
        /// The image title.
        /// </summary>
        private const string ImageTitle = "Image title goes here";

        /// <summary>
        /// The questionnaire text.
        /// </summary>
        private const string QuestionnaireText = "Questionnaire text goes here";

        #endregion

        #region Fields

        /// <summary>
        /// The image guid.
        /// </summary>
        private readonly Guid ImageGuid = Guid.NewGuid();

        /// <summary>
        /// The question guid.
        /// </summary>
        private readonly Guid QuestionGuid = Guid.NewGuid();

        /// <summary>
        /// The now.
        /// </summary>
        private readonly DateTime now = DateTime.UtcNow;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="when_updating_image"/> class.
        /// </summary>
        public when_updating_image()
        {
            Configuration.Configure();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The the_new_image_should_have_the_correct_image_description.
        /// </summary>
        [Then]
        public void the_new_image_should_have_the_correct_image_description()
        {
            Assert.That(this.TheEvent.Description, Is.EqualTo(ImageDescription));
        }

        /// <summary>
        /// The the_new_image_should_have_the_correct_image_id.
        /// </summary>
        [Then]
        public void the_new_image_should_have_the_correct_image_id()
        {
            Assert.That(this.TheEvent.ImageKey, Is.EqualTo(this.ImageGuid));
        }

        /// <summary>
        /// The the_new_image_should_have_the_correct_image_title.
        /// </summary>
        [Then]
        public void the_new_image_should_have_the_correct_image_title()
        {
            Assert.That(this.TheEvent.Title, Is.EqualTo(ImageTitle));
        }

        /// <summary>
        /// The the_new_image_should_have_the_correct_question_id.
        /// </summary>
        [Then]
        public void the_new_image_should_have_the_correct_question_id()
        {
            Assert.That(this.TheEvent.QuestionKey, Is.EqualTo(this.QuestionGuid));
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
            return new object[]
                {
                    new NewQuestionnaireCreated
                        {
                           CreationDate = this.now, PublicKey = this.EventSourceId, Title = QuestionnaireText 
                        }, 
                    new NewQuestionAdded { QuestionType = QuestionType.Text, PublicKey = this.QuestionGuid }, 
                    new ImageUploaded { ImagePublicKey = this.ImageGuid, PublicKey = this.QuestionGuid }
                };
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
        protected override UpdateImageCommand WhenExecuting()
        {
            return new UpdateImageCommand(
                this.EventSourceId, this.QuestionGuid, this.ImageGuid, ImageTitle, ImageDescription);
        }

        #endregion
    }
}