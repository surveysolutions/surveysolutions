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
        public void WhenCommandIsReceived_AllConditionsShouldBeUpdated()
        {
            var innerDocument = new QuestionnaireDocument { Id = "qID" };
            var entity = new Questionnaire(innerDocument);
            var question1 = entity.AddQuestion("The First Question", "", QuestionType.SingleOption, string.Empty, null);
            var question2 = entity.AddQuestion("The Second Question", "", QuestionType.SingleOption, string.Empty, null);
            var question3 = entity.AddQuestion("The Third Question", "stataCap", QuestionType.SingleOption, string.Empty, null);
            var conditions = new Dictionary<Guid, string>
                                 {
                                     {question1.PublicKey, ""},
                                     {question2.PublicKey, "[some_valid]=='expression'"},
                                     {question3.PublicKey, string.Format("[{0}]=='No'", question1.PublicKey)}
                                 };

            var questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            var validator = new Mock<IExpressionExecutor<Questionnaire, bool>>();
            validator.Setup(x => x.Execute(entity, It.IsAny<string>())).Returns(true);

            var handler = new UpdateConditionsHandler(questionnaireRepositoryMock.Object, validator.Object);

            handler.Handle(new UpdateConditionsCommand(entity.QuestionnaireId, conditions, null));

            Assert.AreEqual(conditions[question1.PublicKey], innerDocument.Questions[0].ConditionExpression);
            Assert.AreEqual(conditions[question2.PublicKey], innerDocument.Questions[1].ConditionExpression);
            Assert.AreEqual(conditions[question3.PublicKey], innerDocument.Questions[2].ConditionExpression);
        }
    
        [Test]
        public void WhenCommandIsReceived_ConditionsAreInvalid_NoUpdates()
        {
            var innerDocument = new QuestionnaireDocument { Id = "qID" };
            var entity = new Questionnaire(innerDocument);
            var question1 = entity.AddQuestion("The First Question", "", QuestionType.SingleOption, string.Empty, null);
            var question2 = entity.AddQuestion("The Second Question", "", QuestionType.SingleOption, string.Empty, null);
            var question3 = entity.AddQuestion("The Third Question", "stataCap", QuestionType.SingleOption, string.Empty, null);

            var conditions = new Dictionary<Guid, string>
                                 {
                                     {question1.PublicKey, "@!%R#%"},
                                     {question2.PublicKey, "[some invalid]!~e xpression"}
                                 };

            var questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            var validator = new Mock<IExpressionExecutor<Questionnaire, bool>>();
            validator.Setup(x => x.Execute(entity, It.IsAny<string>())).Returns(false);

            var handler = new UpdateConditionsHandler(questionnaireRepositoryMock.Object, validator.Object);

            handler.Handle(new UpdateConditionsCommand(entity.QuestionnaireId, conditions, null));

            Assert.AreEqual(string.Empty, innerDocument.Questions[0].ConditionExpression);
            Assert.AreEqual(string.Empty, innerDocument.Questions[1].ConditionExpression);
            Assert.AreEqual(string.Empty, innerDocument.Questions[2].ConditionExpression);
        }
    }
}
