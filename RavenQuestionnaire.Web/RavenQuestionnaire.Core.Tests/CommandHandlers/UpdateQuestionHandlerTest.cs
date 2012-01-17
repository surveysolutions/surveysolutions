using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateQuestionHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_QuestionIsUpdatedToRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Questionnaire entity = new Questionnaire(innerDocument);
            Question question = entity.AddQuestion("question", "stataCap", QuestionType.SingleOption, string.Empty, null);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            Mock<IExpressionExecutor<Questionnaire, bool>> validator = new Mock<IExpressionExecutor<Questionnaire, bool>>();
            validator.Setup(x => x.Execute(entity, string.Empty)).Returns(true);
            UpdateQuestionHandler handler = new UpdateQuestionHandler(questionnaireRepositoryMock.Object,
                                                                      validator.Object);
            handler.Handle(new Commands.UpdateQuestionCommand(entity.QuestionnaireId, question.PublicKey,
                                                              "question after update", "export title",  QuestionType.MultyOption, 
                                                              string.Empty, null));

            Assert.True(
                innerDocument.Questions[0].QuestionText == "question after update" &&
                innerDocument.Questions[0].QuestionType == QuestionType.MultyOption);

        }
    }
}
