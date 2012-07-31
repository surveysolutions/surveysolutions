using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Spec;
using Ncqrs.Spec.Fakes;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Events.Questionnaire;

namespace RavenQuestionnaire.Core.Tests.Domain
{
    [Specification]
    public class when_uploading_new_image : OneEventTestFixture<UploadImageCommand, ImageUploaded>
    {

        public when_uploading_new_image()
        {
            Configuration.Configure();
        }

        protected override void RegisterFakesInConfiguration(EnvironmentConfigurationWrapper configuration)
        {
            var clock = new FrozenClock(now);
            configuration.Register<IClock>(clock);
        }

        protected override IEnumerable<object> GivenEvents()
        {
            return new object[]
                       {
                           new NewQuestionnaireCreated()
                               {CreationDate = now, PublicKey = EventSourceId, Title = QuestionnaireText},
                               new NewQuestionAdded(){ QuestionType = QuestionType.Text, PublicKey = QuestionGuid}
                       };
        }

        private DateTime now = DateTime.UtcNow;
        private const string QuestionnaireText = "Questionnaire text goes here";
        private const string ImageTitle = "Image title goes here";
        private const string ImageDescription = "Image description goes here";
        private readonly Guid ImageGuid = Guid.NewGuid();
        private readonly Guid QuestionGuid = Guid.NewGuid();
        #region Overrides of DomainTestFixture<UploadImageCommand>

        protected override UploadImageCommand WhenExecuting()
        {
            return new UploadImageCommand(QuestionGuid, EventSourceId, ImageTitle, ImageDescription, ImageGuid);
        }

        #endregion

        [Then]
        public void the_new_image_should_have_the_correct_question_id()
        {
            Assert.That(TheEvent.PublicKey, Is.EqualTo(QuestionGuid));
        }
        [Then]
        public void the_new_image_should_have_the_correct_image_title()
        {
            Assert.That(TheEvent.Title, Is.EqualTo(ImageTitle));
        }
        [Then]
        public void the_new_image_should_have_the_correct_image_description()
        {
            Assert.That(TheEvent.Description, Is.EqualTo(ImageDescription));
        }
    }
}
