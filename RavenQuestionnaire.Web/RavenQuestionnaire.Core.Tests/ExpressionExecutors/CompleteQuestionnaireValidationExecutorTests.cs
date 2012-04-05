using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
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
            mainGroup.Questions.Add(new CompleteQuestion("q1", QuestionType.Text) {ValidationExpression = "1 = 1", Valid = false});
            mainGroup.Questions.Add(new CompleteQuestion("q2", QuestionType.Text) { ValidationExpression = "2 = 2", Valid = false });
            CompleteQuestionnaireValidationExecutor executor = new CompleteQuestionnaireValidationExecutor(mainGroup);
            executor.Execute(mainGroup);
            Assert.AreEqual(mainGroup.Questions[0].Valid, true);
            Assert.AreEqual(mainGroup.Questions[1].Valid, true);
        }
        [Test]
        public void EvaluateCondition_GroupIsValidAllValidationRulsReturnFalse_AllQuestionsAre_Valid()
        {
            //Mock<ICompleteGroup> questionnaireMoq= new Mock<ICompleteGroup>();
            var mainGroup = new CompleteGroup("root");
            mainGroup.Questions.Add(new CompleteQuestion("q1", QuestionType.Text) { ValidationExpression = "1 != 1", Valid = true });
            mainGroup.Questions.Add(new CompleteQuestion("q2", QuestionType.Text) { ValidationExpression = "2 != 2", Valid = true });
            CompleteQuestionnaireValidationExecutor executor = new CompleteQuestionnaireValidationExecutor(mainGroup);
            executor.Execute(mainGroup);
            Assert.AreEqual(mainGroup.Questions[0].Valid, false);
            Assert.AreEqual(mainGroup.Questions[1].Valid, false);
        }
        [Test]
        public void EvaluateCondition_GroupValidateOnlyOneSubGroup_AllQuestionsAre_Valid()
        {
            //Mock<ICompleteGroup> questionnaireMoq= new Mock<ICompleteGroup>();
            var mainGroup = new CompleteGroup("root");
            var subGroup1 = new CompleteGroup("subgroup1");
            mainGroup.Groups.Add(subGroup1);
            var subGroup2 = new CompleteGroup("subgroup2");
            mainGroup.Groups.Add(subGroup2);
            subGroup1.Questions.Add(new CompleteQuestion("q1", QuestionType.Text) { ValidationExpression = "1 != 1", Valid = true });
            subGroup2.Questions.Add(new CompleteQuestion("q2", QuestionType.Text) { ValidationExpression = "2 != 2", Valid = true });
            CompleteQuestionnaireValidationExecutor executor = new CompleteQuestionnaireValidationExecutor(mainGroup);
            executor.Execute(subGroup1);
            Assert.AreEqual(subGroup1.Questions[0].Valid, false);
            Assert.AreEqual(subGroup2.Questions[0].Valid, true);
        }

        [Test]
        public void EvaluateCondition_GroupIsValidDependingQuestion_AllQuestionsAre_Valid()
        {
            //Mock<ICompleteGroup> questionnaireMoq= new Mock<ICompleteGroup>();
            var mainGroup = new CompleteGroup("root");
            ICompleteQuestion<ICompleteAnswer> q1 = new CompleteQuestion("q1", QuestionType.Text) {Valid = false};
            q1.Answers = new List<ICompleteAnswer>() {new CompleteAnswer() {AnswerValue = 1, Selected = true}};

            mainGroup.Questions.Add(q1);
            ICompleteQuestion<ICompleteAnswer> q2 = new CompleteQuestion("q2", QuestionType.Text) { Valid = false };
            q2.Answers = new List<ICompleteAnswer>() { new CompleteAnswer() { AnswerValue = 2, Selected = true } };

            q1.ValidationExpression = string.Format("[{0}]==2", q2.PublicKey);
            q2.ValidationExpression = string.Format("[{0}]==1", q1.PublicKey);

            mainGroup.Questions.Add(q2);
            CompleteQuestionnaireValidationExecutor executor = new CompleteQuestionnaireValidationExecutor(mainGroup);
            executor.Execute(mainGroup);
            Assert.AreEqual(mainGroup.Questions[0].Valid, true);
            Assert.AreEqual(mainGroup.Questions[1].Valid, true);
        }
    }
}
