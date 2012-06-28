using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Question;
using RavenQuestionnaire.Core.Tests.Utils;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class QuestionTest
    {
        [Test]
        public void WhenSetConditionExpression_ExpressionIsAdded()
        {
            SingleQuestion question = new SingleQuestion();
            question.ConditionExpression="some expression";
            Assert.AreEqual(question.ConditionExpression, "some expression");
        }

        [Test]
        public void ExplicitConversion_ValidQuestion_AllFieldAreConverted()
        {
          //  List<IGroup> groups = new List<IGroup>() { new Group("test") };
            List<IComposite> answers = new List<IComposite>() { new Answer(), new Answer(), new Answer() };

            List<Guid> triggers = new List<Guid>() { Guid.NewGuid() };
            SingleQuestion question = new SingleQuestion(Guid.NewGuid(), "test")
                                    {
                                        ConditionExpression = "expr",
                                        Instructions = "instructions",
                                        AnswerOrder = Order.Random,
                                        StataExportCaption = "stata",
                                        Triggers = triggers,
                                        Children = answers
                                    };
            var target = new CompleteQuestionFactory().ConvertToCompleteQuestion(question);
            var propertiesForCheck =
                typeof(IQuestion).GetPublicPropertiesExcept("PublicKey", "Children","Parent");
            foreach (PropertyInfo publicProperty in propertiesForCheck)
            {

                Assert.AreEqual(publicProperty.GetValue(question, null), publicProperty.GetValue(target, null));
            }
            Assert.AreEqual(question.Children.Count, target.Children.Count);
            for (int i = 0; i < question.Children.Count; i++)
            {
                var answer = target.Find<ICompleteAnswer>(question.Children[i].PublicKey);
                Assert.IsTrue(answer != null);
            }
        }
    }
}
