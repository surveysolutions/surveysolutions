using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Web.Controllers;

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    [TestFixture]
    public class GroupControllerTest
    {
        public Mock<ICommandService> CommandServiceMock { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public GroupController Controller { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            CommandServiceMock = new Mock<ICommandService>();
            ViewRepositoryMock = new Mock<IViewRepository>();
            NcqrsEnvironment.SetDefault<ICommandService>(CommandServiceMock.Object);
            Controller = new GroupController(ViewRepositoryMock.Object);
        }
        [Test]
        public void WhenNewGroupIsSubmittedWIthValidModel_CommandIsSent()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey= key;
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            var question = entity.AddQuestion(Guid.NewGuid(),"question", "stataCap", QuestionType.SingleOption, string.Empty, string.Empty, false,
                Order.AsIs, null, null, Guid.NewGuid());
            var questionView = new QuestionView(innerDocument, question);


            ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(
                        v => v.QuestionnaireId.Equals(key.ToString()))))
                .Returns(new QuestionnaireView(innerDocument));
            Controller.Save(new GroupView() { Title = "test", QuestionnaireId = innerDocument.Id });
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<AddGroupCommand>()), Times.Once());
        }

        [Test]
        public void WhenExistingGroupIsSubmittedWIthValidModel_CommandIsSent()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;
            Group group = new Group("test");
            innerDocument.Children.Add(group);

            var groupView = new GroupView(innerDocument, group);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(new Core.Entities.Questionnaire(innerDocument));
            ViewRepositoryMock.Setup(
              x =>
              x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                  It.Is<QuestionnaireViewInputModel>(
                      v => v.QuestionnaireId.Equals(key.ToString()))))
              .Returns(new QuestionnaireView(innerDocument));


            Controller.Save(groupView);
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<UpdateGroupCommand>()), Times.Once());
        }
        [Test]
        public void When_DeleteGroupIsExecuted()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            Group group = new Group("test");
            innerDocument.Children.Add(group);

            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(entity);

            Controller.Delete(group.PublicKey, entity.QuestionnaireId);
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<DeleteGroupCommand>()), Times.Once());
        }

        [Test]
        public void When_EditQuestionDetailsIsReturned()
        {
            // var output = new QuestionnaireView("questionnairedocuments/qId", "test", DateTime.Now, DateTime.Now, new QuestionView[0]);

            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            Group group = new Group("test");
            innerDocument.Children.Add(group);
            var groupView = new GroupView(innerDocument, group);

            var input = new QuestionViewInputModel(group.PublicKey, entity.QuestionnaireId);

            ViewRepositoryMock.Setup(
                x =>
                x.Load<GroupViewInputModel,GroupView>(
                    It.Is<GroupViewInputModel>(
                        v => v.QuestionnaireId.Equals(input.QuestionnaireId) && v.PublicKey.Equals(input.PublicKey))))
                .Returns(groupView);

            var result = Controller.Edit(group.PublicKey, innerDocument.Id);
            Assert.AreEqual(groupView, ((ViewResult)result).ViewData.Model);
        }
    }
}
