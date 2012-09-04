using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Question;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Tests.ExpressionExecutors
{
    [TestFixture]
    public class QuestionnaireParametersParserTest
    {
        [Test]
        public void ParseCondition_ConditionIsEmpty_ReturnsZeroOfParemeters()
        {
            QuestionnaireDocument doc = new QuestionnaireDocument();
            QuestionnaireParametersParser executor = new QuestionnaireParametersParser();
            var result = executor.Execute(doc, "");
            Assert.AreEqual(result.Count, 0);
        }
        [Test]
        public void ParseCondition_ConditionWithSilgleParameter_ParameterIsReturned()
        {
            QuestionnaireDocument doc = new QuestionnaireDocument();
            var question = new SingleQuestion(Guid.NewGuid(), "some");
            doc.Children.Add(question);
            QuestionnaireParametersParser executor = new QuestionnaireParametersParser();
            var result = executor.Execute(doc, string.Format("[{0}]==1", question.PublicKey));
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].PublicKey, question.PublicKey);
        }
        [Test]
        public void ParseCondition_ConditionWith2Parameter_ParametersareReturned()
        {
            QuestionnaireDocument doc = new QuestionnaireDocument();
            var question1 = new SingleQuestion(Guid.NewGuid(), "some1");
            doc.Children.Add(question1);
            var question2 = new SingleQuestion(Guid.NewGuid(), "some2");
            doc.Children.Add(question2);
            QuestionnaireParametersParser executor = new QuestionnaireParametersParser();
            var result = executor.Execute(doc, string.Format("[{0}]==1 and [{1}]>3", question1.PublicKey, question2.PublicKey));
            Assert.AreEqual(result.Count,2);
            Assert.AreEqual(result[0].PublicKey, question1.PublicKey);
            Assert.AreEqual(result[1].PublicKey, question2.PublicKey);
        }
    }
}
