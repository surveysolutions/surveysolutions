// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyControllerTest.cs" company="">
//   
// </copyright>
// <summary>
//   The survey controller test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.CompleteQuestionnaire.ScreenGroup;
    using Main.Core.View.Group;

    using Moq;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using NUnit.Framework;

    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Security;

    /// <summary>
    /// The survey controller test.
    /// </summary>
    [TestFixture]
    public class SurveyControllerTest
    {
        // public Mock<ICommandInvoker> CommandInvokerMock { get; set; }
        #region Public Properties

        /// <summary>
        /// Gets or sets the authentication.
        /// </summary>
        public Mock<IFormsAuthentication> Authentication { get; set; }

        /// <summary>
        /// Gets or sets the command service mock.
        /// </summary>
        public Mock<ICommandService> CommandServiceMock { get; set; }

        /// <summary>
        /// Gets or sets the info provider.
        /// </summary>
        public Mock<IGlobalInfoProvider> InfoProvider { get; set; }

        /// <summary>
        /// Gets or sets the view repository mock.
        /// </summary>
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }

        #endregion

        // public SurveyController Controller { get; set; }
        #region Public Methods and Operators

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            this.ViewRepositoryMock = new Mock<IViewRepository>();
            this.Authentication = new Mock<IFormsAuthentication>();

            this.InfoProvider = new Mock<IGlobalInfoProvider>();

            this.CommandServiceMock = new Mock<ICommandService>();
            NcqrsEnvironment.SetDefault(this.CommandServiceMock.Object);

            // Controller = new SurveyController(ViewRepositoryMock.Object,  InfoProvider.Object);
        }

        /*[Test]
        public void When_GetCompleteQuestionnaireIsExecutedModelIsReturned()
        {
              var input = new CQGroupedBrowseInputModel();
            var output = new CQGroupedBrowseView(0, 10, 10, new CQGroupItem[0]);
            ViewRepositoryMock.Setup(x => x.Load<CQGroupedBrowseInputModel, CQGroupedBrowseView>(It.IsAny<CQGroupedBrowseInputModel>()))
                .Returns(output);

            var result = Controller.Dashboard();
            Assert.AreEqual(output, result.ViewData.Model);
        }*/

/*        /// <summary>
        /// The participate_ valid id_ form is returned.
        /// </summary>
        [Test]
        public void Participate_ValidId_FormIsReturned()
        {
        }*/

/*        /// <summary>
        /// The question_ valid id_ form is returned.
        /// </summary>
        [Test]
        public void Question_ValidId_FormIsReturned()
        {
            QuestionnaireDocument innerDoc = new QuestionnaireDocument();
            innerDoc.PublicKey = Guid.NewGuid();
            ScreenGroupView template = new ScreenGroupView((CompleteQuestionnaireStoreDocument)innerDoc, new CompleteGroup(), new ScreenNavigation());
            var input = new CompleteQuestionnaireViewInputModel(innerDoc.PublicKey, Guid.NewGuid(), null);
            ViewRepositoryMock.Setup(
            x =>
            x.Load<CompleteQuestionnaireViewInputModel, ScreenGroupView>(
            It.Is<CompleteQuestionnaireViewInputModel>(v => v.CompleteQuestionnaireId.Equals(input.CompleteQuestionnaireId))))
            .Returns(template);
            var result = Controller.Index(innerDoc.PublicKey, null, null, null);
            Assert.AreEqual(result.ViewData.Model.GetType(), typeof(ScreenGroupView));
            Assert.AreEqual(result.ViewData.Model, template);
        }*/

/*        /// <summary>
        /// The save single result_ valid_ form is returned.
        /// </summary>
        [Test]
        public void SaveSingleResult_Valid_FormIsReturned()
        {
            CompleteQuestionView question = new CompleteQuestionView(Guid.NewGuid().ToString(), Guid.NewGuid());
            question.Answers = new CompleteAnswerView[] { new CompleteAnswerView(question.PublicKey, new CompleteAnswer()) };
            Controller.SaveAnswer(
            new CompleteQuestionSettings[] { new CompleteQuestionSettings() { QuestionnaireId = question.QuestionnaireKey, PropogationPublicKey = Guid.NewGuid(), ParentGroupPublicKey = Guid.NewGuid() } },
            new CompleteQuestionView[]
             {
             question
             }
            );
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<SetAnswerCommand>()), Times.Once());
        }*/

        /*/// <summary>
        /// The when_ delete questionnaire is executed.
        /// </summary>
        [Test]
        public void When_DeleteQuestionnaireIsExecuted()
        {
            Controller.Delete(Guid.NewGuid().ToString());
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<DeleteCompleteQuestionnaireCommand>()), Times.Once());
        }*/

        /*/// <summary>
        /// The when_ get questionnaire result is executed.
        /// </summary>
        [Test]
        public void When_GetQuestionnaireResultIsExecuted()
        {
            var innerDoc = new CompleteQuestionnaireStoreDocument();
            innerDoc.PublicKey = Guid.NewGuid();
            innerDoc.CreationDate = DateTime.Now;
            innerDoc.LastEntryDate = DateTime.Now;
            innerDoc.Status = SurveyStatus.Initial;
            innerDoc.Responsible = new UserLight(Guid.NewGuid(), "dummyUser");
            var output = new ScreenGroupView(
                innerDoc, new CompleteGroup(), new ScreenNavigation(), QuestionScope.Interviewer);
            var input = new CompleteQuestionnaireViewInputModel(innerDoc.PublicKey);

            this.ViewRepositoryMock.Setup(
                x =>
                x.Load<CompleteQuestionnaireViewInputModel, ScreenGroupView>(
                    It.Is<CompleteQuestionnaireViewInputModel>(
                        v => v.CompleteQuestionnaireId.Equals(input.CompleteQuestionnaireId)))).Returns(output);

            /*
            StatusDocument statusDoc = new StatusDocument();
            statusDoc.Id = "statusdocuments/sId";
            statusDoc.QuestionnaireId = "questionnairedocuments/cqId";
            statusDoc.Statuses.Add( new StatusItem(){PublicKey = Guid.NewGuid(), Title = "dummy"});

            #1#
            // var result = Controller.Index(output.PublicKey, null, null, null);
            // Assert.AreEqual(output, result.ViewData.Model);
        }*/

        #endregion
    }
}