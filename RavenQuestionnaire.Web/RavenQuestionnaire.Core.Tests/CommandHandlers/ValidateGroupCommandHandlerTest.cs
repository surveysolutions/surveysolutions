using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class ValidateGroupCommandHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_ValiadationServiceIsCalled()
        {
            Mock<ICompleteQuestionnaireRepository> qRepositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            CompleteQuestionnaireDocument questionnaireDoc=new CompleteQuestionnaireDocument();
            CompleteQuestionnaire questionneir = new CompleteQuestionnaire(questionnaireDoc);
            qRepositoryMock.Setup(x => x.Load("completequestionnairedocuments/some_id")).Returns(questionneir);
            Mock<IValildationService> validator = new Mock<IValildationService>();

            ValidateGroupCommandHandler target = new ValidateGroupCommandHandler(qRepositoryMock.Object,
                                                                                 validator.Object);
            var groupGuid = Guid.NewGuid();
            target.Handle(new ValidateGroupCommand("some_id", groupGuid, null, null));

            validator.Verify(x => x.Validate(questionneir, groupGuid, null));
        }
    }
}
