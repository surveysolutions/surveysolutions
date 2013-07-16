using System;
using Main.Core.Domain;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireTests
{
    [TestFixture]
    public class ConstructorTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\t")]
        public void ctor_When_questionnaire_title_is_empty_or_contains_whitespaces_only_Then_throws_DomainException_with_type_QuestionnaireTitleRequired(string emptyTitle)
        {
            // arrange

            // act
            TestDelegate act = () => new Questionnaire(Guid.NewGuid(), emptyTitle);

            // assert
            var domainException = Assert.Throws<DomainException>(act);
            Assert.That(domainException.ErrorType, Is.EqualTo(DomainExceptionType.QuestionnaireTitleRequired));
        }

        [Test]
        public void ctor_When_public_key_specified_Then_raised_NewQuestionnaireCreated_event_with_same_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var publicKey = Guid.NewGuid();

                // act
                new Questionnaire(publicKey, "title");

                // assert
                Assert.That(GetSingleEvent<NewQuestionnaireCreated>(eventContext).PublicKey, Is.EqualTo(publicKey));
            }
        }

        [Test]
        public void ctor_When_title_specified_Then_raised_NewQuestionnaireCreated_event_with_same_title()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var title = "title, the";

                // act
                new Questionnaire(Guid.NewGuid(), title);

                // assert
                Assert.That(GetSingleEvent<NewQuestionnaireCreated>(eventContext).Title, Is.EqualTo(title));
            }
        }

        [Test]
        public void ctor_When_called_Then_raised_NewQuestionnaireCreated_event_with_creation_date_equal_to_current_date()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                var currentDate = new DateTime(2010, 10, 20, 17, 00, 00);
                var clockStub = Mock.Of<IClock>(clock
                    => clock.UtcNow() == currentDate);
                NcqrsEnvironment.SetDefault(clockStub);

                // act
                new Questionnaire(Guid.NewGuid(), "some title");

                // assert
                Assert.That(GetSingleEvent<NewQuestionnaireCreated>(eventContext).CreationDate, Is.EqualTo(currentDate));
            }
        }
    }
}