using System;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    [TestFixture]
    public class SharedPersonsTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void AddSharedPerson_When_shared_personid_is_not_empty_Then_raised_SharedPersonToQuestionnaireAdded_event_with_specified_person_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                Guid personId = Guid.NewGuid();
                string email = "unknown@u.com";

                // act
                questionnaire.AddSharedPerson(personId, email);

                // assert
                var evt = GetSingleEvent<SharedPersonToQuestionnaireAdded>(eventContext);

                Assert.IsNotNull(evt);
                Assert.That(evt.PersonId, Is.EqualTo(personId));
                Assert.That(evt.Email, Is.EqualTo(email));
            }
        }

        [Test]
        public void RemoveSharedPerson_When_shared_personid_is_not_empty_Then_raised_SharedPersonFromQuestionnaireRemoved_event_with_specified_person_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Questionnaire questionnaire = CreateQuestionnaire();
                Guid personId = Guid.NewGuid();

                // act
                questionnaire.RemoveSharedPerson(personId);

                // assert
                Assert.That(GetSingleEvent<SharedPersonFromQuestionnaireRemoved>(eventContext).PersonId, Is.EqualTo(personId));
            }
        }
    }
}