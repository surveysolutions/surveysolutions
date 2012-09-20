using System;
using Core.CAPI.Views.Grouped;
using Main.Core.View;
using Main.Core.View.Answer;
using Main.Core.View.CompleteQuestionnaire;
using Main.Core.View.Question;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
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
        public Mock<IGlobalInfoProvider> InfoProvider { get; set; }
        public Mock<ICommandService> CommandServiceMock { get; set; }
        public SurveyController Controller { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            ViewRepositoryMock = new Mock<IViewRepository>();
            Authentication = new Mock<IFormsAuthentication>();

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
            CompleteQuestionnaireStoreDocument innerDoc = new CompleteQuestionnaireStoreDocument();
            innerDoc.PublicKey = Guid.NewGuid();
            innerDoc.CreationDate = DateTime.Now;
            innerDoc.LastEntryDate = DateTime.Now;
            innerDoc.Status = new SurveyStatus(Guid.NewGuid(), "dummyStatus");
            innerDoc.Responsible = new UserLight(Guid.NewGuid(), "dummyUser");
            var output = new CompleteQuestionnaireMobileView(innerDoc);
            var input = new CompleteQuestionnaireViewInputModel(innerDoc.PublicKey);

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
            var result = Controller.Index(output.PublicKey, null, null, null);
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
            innerDoc.PublicKey = Guid.NewGuid();
            CompleteQuestionnaireMobileView template = new CompleteQuestionnaireMobileView((CompleteQuestionnaireStoreDocument)innerDoc);
            var input = new CompleteQuestionnaireViewInputModel(innerDoc.PublicKey, Guid.NewGuid(), null);
            ViewRepositoryMock.Setup(
               x =>
               x.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                   It.Is<CompleteQuestionnaireViewInputModel>(v => v.CompleteQuestionnaireId.Equals(input.CompleteQuestionnaireId))))
               .Returns(template);
            var result = Controller.Index(innerDoc.PublicKey, null, null, null);
            Assert.AreEqual(result.ViewData.Model.GetType(), typeof(CompleteQuestionnaireMobileView));
            Assert.AreEqual(result.ViewData.Model, template);
        }


        [Test]
        public void SaveSingleResult_Valid_FormIsReturned()
        {
            CompleteQuestionView question = new CompleteQuestionView(Guid.NewGuid().ToString(), Guid.NewGuid());
            question.Answers = new CompleteAnswerView[] {new CompleteAnswerView(question.PublicKey,new CompleteAnswer())};
            Controller.SaveAnswer(
                new CompleteQuestionSettings[] { new CompleteQuestionSettings() { QuestionnaireId = question.QuestionnaireKey, PropogationPublicKey = Guid.NewGuid(), ParentGroupPublicKey = Guid.NewGuid()} },
                new CompleteQuestionView[]
                    {
                        question
                    }
                );
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<SetAnswerCommand>()), Times.Once());
        }
    }
}
