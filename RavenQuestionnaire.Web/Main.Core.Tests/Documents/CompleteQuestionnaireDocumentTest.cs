namespace Main.Core.Tests.Documents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Entities.SubEntities.Question;

    using NUnit.Framework;

    /// <summary>
    /// The complete questionnaire document test.
    /// </summary>
    [TestFixture]
    public class CompleteQuestionnaireDocumentTest
    {
        #region Fields

        /// <summary>
        /// The answer 1 key.
        /// </summary>
        private readonly Guid answer1Key = Guid.NewGuid();

        /// <summary>
        /// The answer 2 key.
        /// </summary>
        private readonly Guid answer2Key = Guid.NewGuid();

        /// <summary>
        /// The question key.
        /// </summary>
        private readonly Guid questionKey = Guid.NewGuid();

        /// <summary>
        /// The document.
        /// </summary>
        private QuestionnaireDocument document;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The check cloneable.
        /// </summary>
        [Test]
        public void CheckCloneable()
        {
            var item = (CompleteQuestionnaireDocument)this.document;

            ICompleteQuestion questionTest01 = item.GetQuestion(this.questionKey, null);
            questionTest01.SetAnswer(new List<Guid> { this.answer1Key }, string.Empty);

            var newDoc = item.Clone() as CompleteQuestionnaireDocument;
            newDoc.ConnectChildsWithParent();

            ICompleteQuestion questionTest1 = newDoc.GetQuestion(this.questionKey, null);
            ICompleteQuestion questionTest2 =
                newDoc.Find<ICompleteQuestion>(q => q.PublicKey == this.questionKey).First();

            Assert.True(ReferenceEquals(questionTest1, questionTest2));

            questionTest1.SetAnswer(new List<Guid> { this.answer2Key }, string.Empty);

            object questionAnswerTest1 = item.GetQuestion(this.questionKey, null).GetAnswerObject();
            object questionAnswerTest2 = newDoc.GetQuestion(this.questionKey, null).GetAnswerObject();

            Assert.AreNotEqual(questionAnswerTest1, questionAnswerTest2);
        }

        /// <summary>
        /// The create objects.
        /// </summary>
        [SetUp]
        public void CreateObjects()
        {
            var doc = new QuestionnaireDocument();
            var mainGroup = new Group();
            var group1 = new Group();
            var question1 = new SingleQuestion(this.questionKey, "Q1");
            var answer1 = new Answer { PublicKey = this.answer1Key, AnswerValue = "1" };
            var answer2 = new Answer { PublicKey = this.answer2Key, AnswerValue = "2" };

            question1.AddAnswer(answer1);
            question1.AddAnswer(answer2);
            group1.Children.Add(question1);
            mainGroup.Children.Add(group1);
            doc.Add(mainGroup, null, null);

            this.document = doc;
        }

        /// <summary>
        /// The _.
        /// </summary>
        [Test]
        public void _()
        {
            var item = (CompleteQuestionnaireDocument)this.document;
            ICompleteQuestion questionTest01 = item.GetQuestion(this.questionKey, null);
            questionTest01.SetAnswer(new List<Guid> { this.answer1Key }, string.Empty);


             
        }

        #endregion
    }
}