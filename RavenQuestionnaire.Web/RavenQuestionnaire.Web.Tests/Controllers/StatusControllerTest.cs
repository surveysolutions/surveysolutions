using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using Ninject.Activation;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Status;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Status;
using RavenQuestionnaire.Web.Controllers;
using RavenQuestionnaire.Web.Tests.Stubs;
using System.Configuration.Provider;

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    [TestFixture]
    public class StatusControllerTest
    {
        public Mock<ICommandInvoker> CommandInvokerMock { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public StatusController Controller { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            CommandInvokerMock = new Mock<ICommandInvoker>();
            ViewRepositoryMock = new Mock<IViewRepository>();
            Controller = new StatusController(CommandInvokerMock.Object, ViewRepositoryMock.Object);
        }



        [Test]
        public void WhenNewStatusIsSubmittedWIthValidModel_CommandIsSent()
        {
            Controller.Save(new StatusBrowseItem() { Title = "testStatus" });
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<CreateNewStatusCommand>()), Times.Once());

        }


        [Test]
        public void When_GetStatusIsExecutedModelIsReturned()
        {
            string questionnaryId = "-1";
            StatusBrowseInputModel input = new StatusBrowseInputModel { QId = questionnaryId };

            var output = new StatusBrowseView(0, 10, 0, new StatusBrowseItem[0], questionnaryId);
            ViewRepositoryMock.Setup(x => x.Load<StatusBrowseInputModel, StatusBrowseView>(input))
                .Returns(output);

            var result = Controller.Index(input);
            Assert.AreEqual(output, result.ViewData.Model);
        }


        [Test]
        public void When_GetQuestionnaireDetailsIsExecuted()
        {
            StatusDocument innerDocument=new StatusDocument();
            innerDocument.Id = "statusdocuments/sId";
            innerDocument.StatusRoles.Add("test", new List<SurveyStatus>() {new SurveyStatus("idtest", "test")});
            var output = new StatusView(innerDocument.Id, "test", true, innerDocument.StatusRoles,"-1", new Dictionary<Guid, FlowRule>());
            var input = new StatusViewInputModel("sId");
            ViewRepositoryMock.Setup(
                x =>
                x.Load<StatusViewInputModel, StatusView>(
                    It.Is<StatusViewInputModel>(v => v.StatusId.Equals(input.StatusId))))
                .Returns(output);
            var result = Controller.Edit(output.Id);
            Assert.AreEqual(output, result.ViewData.Model);
        }

    }
}
