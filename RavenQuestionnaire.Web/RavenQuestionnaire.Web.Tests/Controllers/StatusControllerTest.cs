using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Status;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Status;
using RavenQuestionnaire.Core.Views.Status.Browse;
using RavenQuestionnaire.Core.Views.Status.StatusElement;
using RavenQuestionnaire.Web.Controllers;

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
            Controller.Save(new StatusItemView() { Title = "testStatus" });
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

            var result = Controller.Index(questionnaryId);
            Assert.AreEqual(output, result.ViewData.Model);
        }


        [Test]
        public void When_GetQuestionnaireDetailsIsExecuted()
        {
            StatusDocument innerDocument=new StatusDocument();
            innerDocument.Id = "statusdocuments/sId";

            innerDocument.Statuses.Add(new StatusItem(){Title = "testtt"});

            innerDocument.Statuses[0].StatusRoles.Add("test", new List<SurveyStatus>() {new SurveyStatus(Guid.NewGuid(), "test")});
            var doc = new StatusDocument();//{innerDocument.Id, "test", true, innerDocument.Statuses[0].StatusRoles, "-1", new Dictionary<Guid, FlowRule>()}

            var output = new StatusView(doc);
            var input = new StatusViewInputModel("sId");
            ViewRepositoryMock.Setup(
                x =>
                x.Load<StatusViewInputModel, StatusView>(
                    It.Is<StatusViewInputModel>(v => v.StatusId.Equals(input.StatusId))))
                .Returns(output);
            var result = Controller.Edit("Qid", Guid.Empty);
            Assert.AreEqual(output, result.ViewData.Model);
        }

    }
}
