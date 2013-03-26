// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireControllerTest.cs" company="">
//   
// </copyright>
// <summary>
//   This is a test class for QuestionnaireControllerTest and is intended
//   to contain all QuestionnaireControllerTest Unit Tests
// </summary>
// --------------------------------------------------------------------------------------------------------------------



namespace RavenQuestionnaire.Web.Tests.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Main.Core.Commands.Questionnaire;
    using Main.Core.Documents;
    using Main.Core.View;

    using Moq;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;
    using Main.Core.Export;
    using NUnit.Framework;

    using RavenQuestionnaire.Core.Views.Questionnaire;
    using RavenQuestionnaire.Web.Controllers;

    /// <summary>
    /// This is a test class for QuestionnaireControllerTest and is intended
    /// to contain all QuestionnaireControllerTest Unit Tests
    /// </summary>
    [TestFixture]
    public class QuestionnaireControllerTest
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the command service mock.
        /// </summary>
        public Mock<ICommandService> CommandServiceMock { get; set; }

        /// <summary>
        /// Gets or sets the controller.
        /// </summary>
        public QuestionnaireController Controller { get; set; }

        /// <summary>
        /// Gets or sets the view repository mock.
        /// </summary>
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            this.ViewRepositoryMock = new Mock<IViewRepository>();

            this.CommandServiceMock = new Mock<ICommandService>();
            NcqrsEnvironment.SetDefault(this.CommandServiceMock.Object);
            this.Controller = new QuestionnaireController(this.ViewRepositoryMock.Object);
        }

        /// <summary>
        /// The details_ id is empty_ exception throwed.
        /// </summary>
        [Test]
        public void Details_IdIsEmpty_ExceptionThrowed()
        {
            Assert.Throws<HttpException>(() => this.Controller.Details(Guid.Empty));
        }

        /*[Test]
        public void When_DeleteQuestionnaireIsExecuted()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);

            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            Controller.Delete(entity.QuestionnaireId);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<DeleteQuestionnaireCommand>()), Times.Once());
        }*/

        /// <summary>
        /// The edit_ edit form is returned.
        /// </summary>
        [Test]
        public void Edit_EditFormIsReturned()
        {
            var innerDoc = new QuestionnaireDocument();
            innerDoc.PublicKey = Guid.NewGuid();
            innerDoc.Title = "test";
            innerDoc.CreationDate = DateTime.Now;
            innerDoc.LastEntryDate = DateTime.Now;
            var output = new QuestionnaireView(innerDoc);
            var input = new QuestionnaireViewInputModel(innerDoc.PublicKey);

            this.ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(v => v.QuestionnaireId.Equals(input.QuestionnaireId)))).Returns(
                        output);

            ViewResult result = this.Controller.Edit(output.PublicKey);
            Assert.AreEqual(output, result.ViewData.Model);
        }

        /// <summary>
        /// The when new questionnaire is submitted with valid model_ command is sent.
        /// </summary>
        [Test]
        public void WhenNewQuestionnaireIsSubmittedWithValidModel_CommandIsSent()
        {
            this.Controller.Save(new QuestionnaireView { Title = "test" });
            this.CommandServiceMock.Verify(x => x.Execute(It.IsAny<CreateQuestionnaireCommand>()), Times.Once());
        }

        /// <summary>
        /// The when_ get questionnaire details is executed.
        /// </summary>
        [Test]
        public void When_GetQuestionnaireDetailsIsExecuted()
        {
            var innerDoc = new QuestionnaireDocument();
            innerDoc.PublicKey = Guid.NewGuid();
            innerDoc.Title = "test";
            innerDoc.CreationDate = DateTime.Now;
            innerDoc.LastEntryDate = DateTime.Now;
            var output = new QuestionnaireView(innerDoc);
            var input = new QuestionnaireViewInputModel(innerDoc.PublicKey);

            this.ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(v => v.QuestionnaireId.Equals(input.QuestionnaireId)))).Returns(
                        output);

            ViewResult result = this.Controller.Details(output.PublicKey);
            Assert.AreEqual(output, result.ViewData.Model);
        }

        #endregion
    }
}