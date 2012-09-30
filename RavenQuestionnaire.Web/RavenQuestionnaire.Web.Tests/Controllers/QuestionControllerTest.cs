// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionControllerTest.cs" company="">
//   
// </copyright>
// <summary>
//   The question controller test.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.Core.View.Answer;
using Main.Core.View.Question;

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using Main.Core.Commands.Questionnaire.Question;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities.Question;

    using Moq;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using NUnit.Framework;

    using RavenQuestionnaire.Core;
    using RavenQuestionnaire.Core.Views.Question;
    using RavenQuestionnaire.Core.Views.Questionnaire;
    using RavenQuestionnaire.Web.Controllers;

    /// <summary>
    /// The question controller test.
    /// </summary>
    [TestFixture]
    public class QuestionControllerTest
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the command service mock.
        /// </summary>
        public Mock<ICommandService> CommandServiceMock { get; set; }

        /// <summary>
        /// Gets or sets the controller.
        /// </summary>
        public QuestionController Controller { get; set; }

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
            this.CommandServiceMock = new Mock<ICommandService>();
            this.ViewRepositoryMock = new Mock<IViewRepository>();
            NcqrsEnvironment.SetDefault(this.CommandServiceMock.Object);
            this.Controller = new QuestionController(this.ViewRepositoryMock.Object);
        }

        /// <summary>
        /// The when existing question is submitted w ith valid model_ command is sent.
        /// </summary>
        [Test]
        public void WhenExistingQuestionIsSubmittedWIthValidModel_CommandIsSent()
        {
            var innerDocument = new QuestionnaireDocument();
            innerDocument.PublicKey = Guid.NewGuid();
            var question = new SingleQuestion(Guid.NewGuid(), "question");

            var questionView = new QuestionView(innerDocument, question);
            this.ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(v => v.QuestionnaireId.Equals(innerDocument.PublicKey)))).Returns
                (new QuestionnaireView(innerDocument));

            this.Controller.Save(new[] { questionView }, questionView.Answers, questionView.Triggers);
            this.CommandServiceMock.Verify(x => x.Execute(It.IsAny<ChangeQuestionCommand>()), Times.Once());
        }

        /// <summary>
        /// The when new questione is submitted w ith valid model_ command is sent.
        /// </summary>
        [Test]
        public void WhenNewQuestioneIsSubmittedWIthValidModel_CommandIsSent()
        {
            var innerDocument = new QuestionnaireDocument();
            innerDocument.PublicKey = Guid.NewGuid();

            this.ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(v => v.QuestionnaireId.Equals(innerDocument.PublicKey)))).Returns
                (new QuestionnaireView(innerDocument));
            this.Controller.Save(
                new[] { new QuestionView { Title = "test", QuestionnaireKey = innerDocument.PublicKey } }, 
                new AnswerView[0],
                new Guid[0]);
            this.CommandServiceMock.Verify(x => x.Execute(It.IsAny<AddQuestionCommand>()), Times.Once());
        }

        /*[Test]
        public void When_DeleteQuestionIsExecuted()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = Guid.NewGuid().ToString();
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            var question = entity.AddQuestion(Guid.NewGuid(), "question", "stataCap", QuestionType.SingleOption, string.Empty, string.Empty, false, false, Order.AsIs, null, null, Guid.NewGuid());

            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/" + innerDocument.Id)).Returns(entity);

            Controller.Delete(question.PublicKey, entity.QuestionnaireId);
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<DeleteQuestionCommand>()), Times.Once());
        }*/

        /// <summary>
        /// The when_ edit question details is returned.
        /// </summary>
        [Test]
        public void When_EditQuestionDetailsIsReturned()
        {
            // var output = new QuestionnaireView("questionnairedocuments/qId", "test", DateTime.Now, DateTime.Now, new QuestionView[0]);
            var innerDocument = new QuestionnaireDocument();
            var question = new SingleQuestion(Guid.NewGuid(), "question");
            var questionView = new QuestionView(innerDocument, question);

            var input = new QuestionViewInputModel(question.PublicKey, innerDocument.PublicKey);

            this.ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionViewInputModel, QuestionView>(
                    It.Is<QuestionViewInputModel>(
                        v => v.QuestionnaireId.Equals(input.QuestionnaireId) && v.PublicKey.Equals(input.PublicKey)))).
                Returns(questionView);

            ActionResult result = this.Controller.Edit(question.PublicKey, innerDocument.PublicKey);
            Assert.AreEqual(questionView, ((PartialViewResult)result).ViewData.Model);
        }

        #endregion
    }
}