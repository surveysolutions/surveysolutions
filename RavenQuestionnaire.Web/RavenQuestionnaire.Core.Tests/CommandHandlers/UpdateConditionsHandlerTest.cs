#region

using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;

#endregion

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateConditionsHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_ConditionsAreUpdatedToRepository()
        {
            var innerDocument = new QuestionnaireDocument {Id = "qID"};
            var entity = new Questionnaire(innerDocument);
            var question1 = entity.AddQuestion("The First Question", "", QuestionType.SingleOption, string.Empty, null);
            var question2 = entity.AddQuestion("The Second Question", "", QuestionType.SingleOption, string.Empty, null);
            var question3 = entity.AddQuestion("The Third Question", "stataCap", QuestionType.SingleOption, string.Empty, null);
            string validCondition = string.Format("[{0}]=='No'", question1.PublicKey);
            var conditions = new Dictionary<Guid, string>
                                 {
                                     {question1.PublicKey, ""},
                                     {question2.PublicKey, "[some invalid]!~e xpression"},
                                     {question3.PublicKey, validCondition}
                                 };

            var questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            var validator = new Mock<IExpressionExecutor<Questionnaire, bool>>();
            //validator.Setup(x => x.Execute(entity, y)).Returns(true);

            var handler = new UpdateConditionsHandler(questionnaireRepositoryMock.Object, validator.Object);

            handler.Handle(new UpdateConditionsCommand(entity.QuestionnaireId, conditions, null));

            Assert.AreEqual(string.Empty,innerDocument.Questions[0].ConditionExpression);
            Assert.AreEqual(string.Empty,innerDocument.Questions[1].ConditionExpression);
            Assert.AreEqual(validCondition, innerDocument.Questions[2].ConditionExpression);
        }
    }
}