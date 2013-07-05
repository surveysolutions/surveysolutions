namespace RavenQuestionnaire.Core.Tests.ExpressionExecutors
{
    using System.Collections.Generic;

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Entities.SubEntities.Complete.Question;
    using Main.Core.ExpressionExecutors;

    using NUnit.Framework;

    /// <summary>
    /// The complete questionnaire validation executor tests.
    /// </summary>
    [TestFixture]
    public class CompleteQuestionnaireValidationExecutorTests
    {
        #region Public Methods and Operators

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            // iteratorContainerMock = new Mock<IIteratorContainer>();
        }

        /// <summary>
        /// The evaluate condition_ disabled question_ result is true.
        /// </summary>
        [Test]
        public void EvaluateCondition_DisabledQuestion_ResultISTrue()
        {
            var mainGroup = new CompleteGroup("root");
            var q1 = new SingleCompleteQuestion("q1") { Enabled = false };
            mainGroup.Children.Add(q1);

            var doc = new CompleteQuestionnaireDocument();
            doc.Children.Add(mainGroup);
            var executor = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
            var result = executor.Execute(q1);
            Assert.AreEqual(result, true);
        }

        /// <summary>
        /// The evaluate condition_ empty question mandatory_ result is false.
        /// </summary>
        [Test]
        public void EvaluateCondition_EmptyQuestionMandatory_ResultISFalse()
        {
            var mainGroup = new CompleteGroup("root");
            var q1 = new SingleCompleteQuestion("q1") { Enabled = true, Mandatory = true };
            mainGroup.Children.Add(q1);
            var doc = new CompleteQuestionnaireDocument();
            doc.Children.Add(mainGroup);

            var executor = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
            var result = executor.Execute(q1);
            Assert.AreEqual(result, false);
        }

        /// <summary>
        /// The evaluate condition_ empty question not mandatory_ result is true.
        /// </summary>
        [Test]
        public void EvaluateCondition_EmptyQuestionNotMandatory_ResultISTrue()
        {
            var mainGroup = new CompleteGroup("root");
            var q1 = new SingleCompleteQuestion("q1") { Enabled = true };
            mainGroup.Children.Add(q1);
            var doc = new CompleteQuestionnaireDocument();
            doc.Children.Add(mainGroup);

            var executor = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
            var result = executor.Execute(q1);
            Assert.AreEqual(result, true);
        }

        /// <summary>
        /// The evaluate condition_ group is valid all validation ruls return false_ all questions are_ valid.
        /// </summary>
        [Test]
        public void EvaluateCondition_GroupIsValidAllValidationRulsReturnFalse_AllQuestionsAre_Valid()
        {
            // Mock<ICompleteGroup> questionnaireMoq= new Mock<ICompleteGroup>();
            var mainGroup = new CompleteGroup("root");
            var q1 = new TextCompleteQuestion("q1") { ValidationExpression = "1 != 1", Valid = true };
            q1.SetAnswer(null, "hello");
            mainGroup.Children.Add(q1);
            var q2 = new TextCompleteQuestion("q2") { ValidationExpression = "2 != 2", Valid = true };
            q2.SetAnswer(null, "2");
            mainGroup.Children.Add(q2);

            var doc = new CompleteQuestionnaireDocument();
            doc.Children.Add(mainGroup);

            var executor = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
            executor.Execute();
            Assert.AreEqual(((ICompleteQuestion)mainGroup.Children[0]).Valid, false);
            Assert.AreEqual(((ICompleteQuestion)mainGroup.Children[1]).Valid, false);
        }

        /// <summary>
        /// The evaluate condition_ group is valid all validation ruls return true_ all questions are_ valid.
        /// </summary>
        [Test]
        public void EvaluateCondition_GroupIsValidAllValidationRulsReturnTrue_AllQuestionsAre_Valid()
        {
            // Mock<ICompleteGroup> questionnaireMoq= new Mock<ICompleteGroup>();
            var mainGroup = new CompleteGroup("root");
            mainGroup.Children.Add(new SingleCompleteQuestion("q1") { ValidationExpression = "1 = 1", Valid = false });
            mainGroup.Children.Add(new SingleCompleteQuestion("q2") { ValidationExpression = "2 = 2", Valid = false });

            var doc = new CompleteQuestionnaireDocument();
            doc.Children.Add(mainGroup);

            var executor = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
            executor.Execute();
            Assert.AreEqual(((ICompleteQuestion)mainGroup.Children[0]).Valid, true);
            Assert.AreEqual(((ICompleteQuestion)mainGroup.Children[1]).Valid, true);
        }

        /// <summary>
        /// The evaluate condition_ group is valid depending question_ all questions are_ valid.
        /// </summary>
        [Test]
        public void EvaluateCondition_GroupIsValidDependingQuestion_AllQuestionsAre_Valid()
        {
            // Mock<ICompleteGroup> questionnaireMoq= new Mock<ICompleteGroup>();
            var mainGroup = new CompleteGroup("root");
            var q1 = new SingleCompleteQuestion("q1") { Valid = false };
            q1.Answers = new List<IAnswer> { new CompleteAnswer { AnswerValue = "1", Selected = true } };

            mainGroup.Children.Add(q1);
            var q2 = new SingleCompleteQuestion("q2") { Valid = false };
            q2.Answers = new List<IAnswer> { new CompleteAnswer { AnswerValue = "2", Selected = true } };

            q1.ValidationExpression = string.Format("[{0}]==2", q2.PublicKey);
            q2.ValidationExpression = string.Format("[{0}]==1", q1.PublicKey);

            mainGroup.Children.Add(q2);
            var doc = new CompleteQuestionnaireDocument();
            doc.Children.Add(mainGroup);

            var executor = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
            executor.Execute();
            Assert.AreEqual(((ICompleteQuestion)mainGroup.Children[0]).Valid, true);
            Assert.AreEqual(((ICompleteQuestion)mainGroup.Children[0]).Valid, true);
        }

        /// <summary>
        /// The evaluate condition_ group validate only one sub group_ all questions are_ valid.
        /// </summary>
        [Test]
        public void EvaluateCondition_GroupValidateOnlyOneSubGroup_AllQuestionsAre_Valid()
        {
            // Mock<ICompleteGroup> questionnaireMoq= new Mock<ICompleteGroup>();
            var mainGroup = new CompleteGroup("root");
            var subGroup1 = new CompleteGroup("subgroup1");
            mainGroup.Children.Add(subGroup1);
            var subGroup2 = new CompleteGroup("subgroup2");
            mainGroup.Children.Add(subGroup2);

            var q1 = new TextCompleteQuestion("q1") { ValidationExpression = "1 != 1", Valid = true };
            q1.SetAnswer(null, "hello");
            subGroup1.Children.Add(q1);
            var q2 = new TextCompleteQuestion("q2") { ValidationExpression = "2 != 2", Valid = true };
            q2.SetAnswer(null, "2");
            subGroup2.Children.Add(q2);

            var doc = new CompleteQuestionnaireDocument();
            doc.Children.Add(mainGroup);

            var executor = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
            executor.Execute(subGroup1);
            Assert.AreEqual(((ICompleteQuestion)subGroup1.Children[0]).Valid, false);
            Assert.AreEqual(((ICompleteQuestion)subGroup2.Children[0]).Valid, true);
        }

        /// <summary>
        /// The evaluate condition_ not empty question validation condition i contains this in valid condition_ result is false.
        /// </summary>
        [Test]
        public void EvaluateCondition_NotEmptyQuestionValidationConditionIContainsThisInValidCondition_ResultISFalse()
        {
            var mainGroup = new CompleteGroup("root");
            var q1 = new TextCompleteQuestion("q1")
                {
                   Enabled = true, Mandatory = false, ValidationExpression = "[this]!='Answer'" 
                };
            q1.SetAnswer(null, "Answer");
            mainGroup.Children.Add(q1);

            var doc = new CompleteQuestionnaireDocument();
            doc.Children.Add(mainGroup);

            var executor = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
            var result = executor.Execute(q1);
            Assert.AreEqual(result, false);
        }

        /// <summary>
        /// The evaluate condition_ not empty question validation condition i contains this valid condition_ result is true.
        /// </summary>
        [Test]
        public void EvaluateCondition_NotEmptyQuestionValidationConditionIContainsThisValidCondition_ResultISTrue()
        {
            var mainGroup = new CompleteGroup("root");
            var q1 = new TextCompleteQuestion("q1")
                {
                   Enabled = true, Mandatory = false, ValidationExpression = "[this]=='Answer'" 
                };
            q1.SetAnswer(null, "Answer");
            mainGroup.Children.Add(q1);

            var doc = new CompleteQuestionnaireDocument();
            doc.Children.Add(mainGroup);

            var executor = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
            var result = executor.Execute(q1);
            Assert.AreEqual(result, true);
        }

        /// <summary>
        /// The evaluate condition_ not empty question validation condition is empty_ result is true.
        /// </summary>
        [Test]
        public void EvaluateCondition_NotEmptyQuestionValidationConditionIsEmpty_ResultISTrue()
        {
            var mainGroup = new CompleteGroup("root");
            var q1 = new TextCompleteQuestion("q1")
                {
                   Enabled = true, Mandatory = false, ValidationExpression = string.Empty 
                };
            q1.SetAnswer(null, "answer");
            mainGroup.Children.Add(q1);
            var doc = new CompleteQuestionnaireDocument();
            doc.Children.Add(mainGroup);

            var executor = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
            var result = executor.Execute(q1);
            Assert.AreEqual(result, true);
        }

        #endregion
    }
}