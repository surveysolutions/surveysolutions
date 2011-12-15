using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class AddNewQuestionHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_NewQuestionnIsAddedToRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Questionnaire entity = new Questionnaire(innerDocument);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);
            AddNewQuestionHandler handler = new AddNewQuestionHandler(questionnaireRepositoryMock.Object);
            AnswerView[] answers = new AnswerView[]{ new AnswerView(){ AnswerText = "answer", AnswerType = AnswerType.Text} };
            handler.Handle(new Commands.AddNewQuestionCommand("test", QuestionType.SingleOption, entity.QuestionnaireId,
                                                              answers));

            questionnaireRepositoryMock.Verify(x => x.Load("questionnairedocuments/qID"), Times.Once());

        }
    }
}
