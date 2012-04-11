using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Tests.Utils;

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

        [Test]
        public void ExplicitConversion_ValidQuestion_AllFieldAreConverted()
        {
          //  List<IGroup> groups = new List<IGroup>() { new Group("test") };
            List<IAnswer> answers = new List<IAnswer>() { new Answer(), new Answer(), new Answer() };

            List<Guid> triggers = new List<Guid>() { Guid.NewGuid() };
            Question question = new Question("test", QuestionType.MultyOption)
                                    {
                                        ConditionExpression = "expr",
                                        Instructions = "instructions",
                                        AnswerOrder = Order.Random,
                                        StataExportCaption = "stata",
                                        Triggers = triggers,
                                        Answers = answers
                                    };
            CompleteQuestion target = (CompleteQuestion)question;
            var propertiesForCheck =
                typeof(IQuestion).GetPublicPropertiesExcept("PublicKey");
            foreach (PropertyInfo publicProperty in propertiesForCheck)
            {

                Assert.AreEqual(publicProperty.GetValue(question, null), publicProperty.GetValue(target, null));
            }
            Assert.AreEqual(question.Answers.Count, target.Answers.Count);
            for (int i = 0; i < question.Answers.Count; i++)
            {
                var answer = target.Find<ICompleteAnswer>(question.Answers[i].PublicKey);
                Assert.IsTrue(answer != null);
            }
        }
    }
}
