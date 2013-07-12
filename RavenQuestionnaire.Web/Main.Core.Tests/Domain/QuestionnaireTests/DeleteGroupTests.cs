using System;
using Main.Core.Domain;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using Ncqrs.Spec;

namespace Main.Core.Tests.Domain.QuestionnaireTests
{
    [TestFixture]
    public class DeleteGroupTests : QuestionnaireARTestContext {

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void DeleteGroup_When_group_public_key_specified_Then_raised_GroupDeleted_event_with_same_group_public_key()
        {
            using (var eventContext = new EventContext())
            {
                // arrange
                Guid groupPublicKey = Guid.NewGuid();
                QuestionnaireAR questionnaire = QuestionnaireARTestContext.CreateQuestionnaireARWithOneGroup(groupId: groupPublicKey);

                // act
                Guid parentPublicKey = Guid.NewGuid();
                questionnaire.NewDeleteGroup(groupPublicKey);

                // assert
                Assert.That(QuestionnaireARTestContext.GetSingleEvent<GroupDeleted>(eventContext).GroupPublicKey, Is.EqualTo(groupPublicKey));
            }
        }
    }
}