using System;
using System.Web.Mvc;
using Main.Core.View;
using Main.Core.View.Question;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Main.Core;
using Main.Core.Commands.Questionnaire.Group;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

using Core.CAPI.Views.Group;
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
         //   Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            var question = new SingleQuestion(Guid.NewGuid(),"question");
            var questionView = new QuestionView(innerDocument, question);


            ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(
                        v => v.QuestionnaireId.Equals(key.ToString()))))
                .Returns(new QuestionnaireView(innerDocument));
            Controller.Save(new GroupView() { Title = "test", QuestionnaireKey = innerDocument.PublicKey});
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<AddGroupCommand>()), Times.Once());
        }

        [Test]
        public void When_EditQuestionDetailsIsReturned()
        {
            // var output = new QuestionnaireView("questionnairedocuments/qId", "test", DateTime.Now, DateTime.Now, new QuestionView[0]);

            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;
           
            Group group = new Group("test");
            innerDocument.Children.Add(group);
            var groupView = new GroupView(innerDocument, group);

            var input = new QuestionViewInputModel(group.PublicKey, innerDocument.PublicKey);

            ViewRepositoryMock.Setup(
                x =>
                x.Load<GroupViewInputModel,GroupView>(
                    It.Is<GroupViewInputModel>(
                        v => v.QuestionnaireId.Equals(input.QuestionnaireId) && v.PublicKey.Equals(input.PublicKey))))
                .Returns(groupView);

            var result = Controller.Edit(group.PublicKey, innerDocument.PublicKey);
            Assert.AreEqual(groupView, ((ViewResult)result).ViewData.Model);
        }
    }
}
