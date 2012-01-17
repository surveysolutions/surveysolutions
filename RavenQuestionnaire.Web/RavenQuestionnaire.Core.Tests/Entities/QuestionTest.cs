using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class QuestionTest
    {
        [Test]
        public void WhenSetConditionExpression_ExpressionIsAdded()
        {
            Question question = new Question();
            question.ConditionExpression="some expression";
            Assert.AreEqual(question.ConditionExpression, "some expression");
        }
       
      
    }
}
