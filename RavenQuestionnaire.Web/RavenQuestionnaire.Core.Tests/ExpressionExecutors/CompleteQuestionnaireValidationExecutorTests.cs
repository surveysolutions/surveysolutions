using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Tests.ExpressionExecutors
{
    [TestFixture]
    public class CompleteQuestionnaireValidationExecutorTests
    {
        [SetUp]
        public void CreateObjects()
        {
            // iteratorContainerMock = new Mock<IIteratorContainer>();
        }
        [Test]
        public void EvaluateCondition_GroupIsValidAllValidationRulsReturnTrue_AllQuestionsAre_Valid()
        {
            //Mock<ICompleteGroup> questionnaireMoq= new Mock<ICompleteGroup>();
            var mainGroup = new CompleteGroup("root");
            mainGroup.Children.Add(new SingleCompleteQuestion("q1") { ValidationExpression = "1 = 1", Valid = false });
            mainGroup.Children.Add(new SingleCompleteQuestion("q2") { ValidationExpression = "2 = 2", Valid = false });
            CompleteQuestionnaireValidationExecutor executor = new CompleteQuestionnaireValidationExecutor(mainGroup);
            executor.Execute(mainGroup);
            Assert.AreEqual(((ICompleteQuestion)mainGroup.Children[0]).Valid, true);
            Assert.AreEqual(((ICompleteQuestion)mainGroup.Children[1]).Valid, true);
        }
        [Test]
        public void EvaluateCondition_GroupIsValidAllValidationRulsReturnFalse_AllQuestionsAre_Valid()
        {
            //Mock<ICompleteGroup> questionnaireMoq= new Mock<ICompleteGroup>();
            var mainGroup = new CompleteGroup("root");
            mainGroup.Children.Add(new SingleCompleteQuestion("q1") { ValidationExpression = "1 != 1", Valid = true });
            mainGroup.Children.Add(new SingleCompleteQuestion("q2") { ValidationExpression = "2 != 2", Valid = true });
            CompleteQuestionnaireValidationExecutor executor = new CompleteQuestionnaireValidationExecutor(mainGroup);
            executor.Execute(mainGroup);
            Assert.AreEqual(((ICompleteQuestion)mainGroup.Children[0]).Valid, false);
            Assert.AreEqual(((ICompleteQuestion)mainGroup.Children[1]).Valid, false);
        }
        [Test]
        public void EvaluateCondition_GroupValidateOnlyOneSubGroup_AllQuestionsAre_Valid()
        {
            //Mock<ICompleteGroup> questionnaireMoq= new Mock<ICompleteGroup>();
            var mainGroup = new CompleteGroup("root");
            var subGroup1 = new CompleteGroup("subgroup1");
            mainGroup.Children.Add(subGroup1);
            var subGroup2 = new CompleteGroup("subgroup2");
            mainGroup.Children.Add(subGroup2);
            subGroup1.Children.Add(new SingleCompleteQuestion("q1") { ValidationExpression = "1 != 1", Valid = true });
            subGroup2.Children.Add(new SingleCompleteQuestion("q2") { ValidationExpression = "2 != 2", Valid = true });
            CompleteQuestionnaireValidationExecutor executor = new CompleteQuestionnaireValidationExecutor(mainGroup);
            executor.Execute(subGroup1);
            Assert.AreEqual(((ICompleteQuestion)subGroup1.Children[0]).Valid, false);
            Assert.AreEqual(((ICompleteQuestion)subGroup2.Children[0]).Valid, true);
        }

        [Test]
        public void EvaluateCondition_GroupIsValidDependingQuestion_AllQuestionsAre_Valid()
        {
            //Mock<ICompleteGroup> questionnaireMoq= new Mock<ICompleteGroup>();
            var mainGroup = new CompleteGroup("root");
            ICompleteQuestion q1 = new SingleCompleteQuestion("q1") { Valid = false };
            q1.Children = new List<IComposite>() { new CompleteAnswer() { AnswerValue = 1, Selected = true } };

            mainGroup.Children.Add(q1);
            ICompleteQuestion q2 = new SingleCompleteQuestion("q2") { Valid = false };
            q2.Children = new List<IComposite>() { new CompleteAnswer() { AnswerValue = 2, Selected = true } };

            q1.ValidationExpression = string.Format("[{0}]==2", q2.PublicKey);
            q2.ValidationExpression = string.Format("[{0}]==1", q1.PublicKey);

            mainGroup.Children.Add(q2);
            CompleteQuestionnaireValidationExecutor executor = new CompleteQuestionnaireValidationExecutor(mainGroup);
            executor.Execute(mainGroup);
            Assert.AreEqual(((ICompleteQuestion)mainGroup.Children[0]).Valid, true);
            Assert.AreEqual(((ICompleteQuestion)mainGroup.Children[0]).Valid, true);
        }
    }
}
