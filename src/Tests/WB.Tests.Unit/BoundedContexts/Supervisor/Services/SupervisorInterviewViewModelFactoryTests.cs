using System;
using System.Collections.ObjectModel;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorInterviewViewModelFactory))]
    public class SupervisorInterviewViewModelFactoryTests
    {
        [Test]
        public void should_return_list_returned_by_GetChildEntityIds_method()
        {
            var targetIdentity = Create.Identity();

            var questionnaire = new Mock<IQuestionnaire>();
            var questionnaireArray = new ReadOnlyCollection<Guid>(Array.Empty<Guid>());
            questionnaire.Setup(x => x.GetChildEntityIds(targetIdentity.Id))
                .Returns(questionnaireArray);
            var factory = Create.Service.SupervisorInterviewViewModelFactory();
            
            // Act
            var underlyingInterviewerEntities = factory.GetUnderlyingInterviewerEntities(targetIdentity, questionnaire.Object);

            // Assert
            Assert.That(underlyingInterviewerEntities, Is.SameAs(questionnaireArray));
        }
    }
}
