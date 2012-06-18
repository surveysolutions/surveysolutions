using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Status;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Status;
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
            Controller.Save(new StatusItemView() { Title = "testStatus" , StatusId = "1", QuestionnaireId = "1"});
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<CreateNewStatusCommand>()), Times.Once());

        }


        [Test]
        public void When_GetStatusIsExecutedModelIsReturned()
        {
            string questionnaryId = "-1";
            StatusViewInputModel input = new StatusViewInputModel(questionnaryId) ;

            var output = new StatusView() {QuestionnaireId = questionnaryId};

            ViewRepositoryMock.Setup(
                x =>
                x.Load<StatusViewInputModel, StatusView>(
                    It.Is<StatusViewInputModel>(v => v.QId.Equals(input.QId))))
                .Returns(output);

            var result = Controller.Details(questionnaryId);
            Assert.AreEqual(output, result.ViewData.Model);
        }


        [Test]
        public void When_GetQuestionnaireDetailsIsExecuted()
        {
            StatusDocument innerDocument=new StatusDocument();
            innerDocument.Id = "statusdocuments/sId";
            innerDocument.QuestionnaireId = "questionnairedocuments/sId";

            Guid status0PublicKey = Guid.NewGuid();
            Guid status1PublicKey = Guid.NewGuid();


            innerDocument.Statuses.Add(new StatusItem() { PublicKey = status1PublicKey, Title = "test1" });
            innerDocument.Statuses.Add(new StatusItem() { PublicKey = status0PublicKey , Title = "testtt0"});
            

            innerDocument.Statuses[0].StatusRoles.Add("test", new List<SurveyStatus>() { new SurveyStatus(status0PublicKey, "testtt0") });
            innerDocument.Statuses[1].StatusRoles.Add("test", new List<SurveyStatus>() { new SurveyStatus(status1PublicKey, "test1") });

            
            var doc = new StatusDocument()
                          {
                              Id = innerDocument.Id,
                              Statuses = innerDocument.Statuses,
                              QuestionnaireId = innerDocument.QuestionnaireId
                          };

            var output = new StatusView(doc);
            var input = new StatusViewInputModel("Qid");

            ViewRepositoryMock.Setup(
                x =>
                x.Load<StatusViewInputModel, StatusView>(
                    It.Is<StatusViewInputModel>(v => v.QId.Equals(input.QId))))
                .Returns(output);

            var result = Controller.Edit("Qid", status1PublicKey);

            Assert.AreEqual(output.StatusElements[0], result.ViewData.Model);
        }

    }
}
