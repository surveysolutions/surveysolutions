using System;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
using RavenQuestionnaire.Core.Views.Question;
using Web.CAPI.Controllers;
using Web.CAPI.Models;

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    [TestFixture]
    public class SurveyControllerTest
    {
        //public Mock<ICommandInvoker> CommandInvokerMock { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public Mock<IFormsAuthentication> Authentication { get; set; }
        public Mock<IBagManager> BagManager { get; set; }
        public Mock<IGlobalInfoProvider> InfoProvider { get; set; }
        public Mock<ICommandService> CommandServiceMock { get; set; }
        public SurveyController Controller { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            ViewRepositoryMock = new Mock<IViewRepository>();
            Authentication = new Mock<IFormsAuthentication>();
            BagManager = new Mock<IBagManager>();

            InfoProvider = new Mock<IGlobalInfoProvider>();

            CommandServiceMock = new Mock<ICommandService>();
            NcqrsEnvironment.SetDefault<ICommandService>(CommandServiceMock.Object);
            Controller = new SurveyController(ViewRepositoryMock.Object,  InfoProvider.Object);
        }

        [Test]
        public void When_GetCompleteQuestionnaireIsExecutedModelIsReturned()
        {
          //  var input = new CQGroupedBrowseInputModel();
            var output = new CQGroupedBrowseView(0,10,10,new CQGroupItem[0]);
            ViewRepositoryMock.Setup(x => x.Load<CQGroupedBrowseInputModel, CQGroupedBrowseView>(It.IsAny<CQGroupedBrowseInputModel>()))
                .Returns(output);

            var result = Controller.Dashboard();
            Assert.AreEqual(output, result.ViewData.Model);
        }
        [Test]
        public void When_GetQuestionnaireResultIsExecuted()
        {
            CompleteQuestionnaireDocument innerDoc = new CompleteQuestionnaireDocument();
            innerDoc.Id = "completequestionnairedocuments/cqId";
            innerDoc.CreationDate = DateTime.Now;
            innerDoc.LastEntryDate = DateTime.Now;
            innerDoc.Status = new SurveyStatus(Guid.NewGuid(), "dummyStatus");
            innerDoc.Responsible = new UserLight("-1", "dummyUser");
            var output = new CompleteQuestionnaireMobileView(innerDoc);
            var input = new CompleteQuestionnaireViewInputModel("cqId");

            ViewRepositoryMock.Setup(
                x =>
                x.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                    It.Is<CompleteQuestionnaireViewInputModel>(v => v.CompleteQuestionnaireId.Equals(input.CompleteQuestionnaireId))))
                .Returns(output);

/*
            StatusDocument statusDoc = new StatusDocument();
            statusDoc.Id = "statusdocuments/sId";
            statusDoc.QuestionnaireId = "questionnairedocuments/cqId";
            statusDoc.Statuses.Add( new StatusItem(){PublicKey = Guid.NewGuid(), Title = "dummy"});

            */
            var result = Controller.Index(output.Id, null, null, null);
            Assert.AreEqual(output, result.ViewData.Model);
        }
        [Test]
        public void When_DeleteQuestionnaireIsExecuted()
        {
            Controller.Delete(Guid.NewGuid().ToString());

            CommandServiceMock.Verify(x => x.Execute(It.IsAny<DeleteCompleteQuestionnaireCommand>()), Times.Once());
        }

        
        [Test]
        public void Participate_ValidId_FormIsReturned()
        {
        
        }

        [Test]
        public void Question_ValidId_FormIsReturned()
        {
            QuestionnaireDocument innerDoc = new QuestionnaireDocument();
            innerDoc.Id = "questionnairedocuments/cqId";
            CompleteQuestionnaireMobileView template = new CompleteQuestionnaireMobileView((CompleteQuestionnaireDocument)innerDoc);
            var input = new CompleteQuestionnaireViewInputModel("cqId", Guid.NewGuid(),null);
            ViewRepositoryMock.Setup(
               x =>
               x.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                   It.Is<CompleteQuestionnaireViewInputModel>(v => v.CompleteQuestionnaireId.Equals(input.CompleteQuestionnaireId))))
               .Returns(template);
            var result = Controller.Index("cqId", null,null,null);
            Assert.AreEqual(result.ViewData.Model.GetType(), typeof(CompleteQuestionnaireMobileView));
            Assert.AreEqual(result.ViewData.Model, template);
        }


        [Test]
        public void SaveSingleResult_Valid_FormIsReturned()
        {
            CompleteQuestionView question = new CompleteQuestionView(Guid.NewGuid().ToString(), Guid.NewGuid());
            question.Answers = new CompleteAnswerView[] {new CompleteAnswerView(question.PublicKey,new CompleteAnswer())};
            Controller.SaveAnswer(
                new CompleteQuestionSettings[] { new CompleteQuestionSettings() { QuestionnaireId = question.QuestionnaireId, PropogationPublicKey = Guid.NewGuid(), ParentGroupPublicKey = Guid.NewGuid()} },
                new CompleteQuestionView[]
                    {
                        question
                    }
                );
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<SetAnswerCommand>()),
                                      Times.Once());
        }
    }
}
