namespace Main.Core.Tests.Domain
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Question;

    /// <summary>
    /// The test data configurator.
    /// </summary>
    public class TestDataConfigurator
    {
        #region Fields

        /// <summary>
        /// The answer 1 key.
        /// </summary>
        public readonly Guid Answer1Key = Guid.NewGuid();

        /// <summary>
        /// The answer 2 key.
        /// </summary>
        public readonly Guid Answer2Key = Guid.NewGuid();

        /// <summary>
        /// The answer 1 key.
        /// </summary>
        public readonly Guid AutoAnswer1Key = Guid.NewGuid();

        /// <summary>
        /// The answer 2 key.
        /// </summary>
        public readonly Guid AutoAnswer2Key = Guid.NewGuid();

        /// <summary>
        /// The question key.
        /// </summary>
        public readonly Guid AutoGroupKey = Guid.NewGuid();

        /// <summary>
        /// The question key.
        /// </summary>
        public readonly Guid AutoPropQuestionKey = Guid.NewGuid();

        /// <summary>
        /// The question key.
        /// </summary>
        public readonly Guid AutoQuestionKey = Guid.NewGuid();

        /// <summary>
        /// The question key.
        /// </summary>
        public readonly Guid MainGroupKey = Guid.NewGuid();

        /// <summary>
        /// The question key.
        /// </summary>
        public readonly Guid QuestionKey = Guid.NewGuid();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataConfigurator"/> class.
        /// </summary>
        public TestDataConfigurator()
        {
            this.Document = this.GenerateQuestionnaireDocument();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the document.
        /// </summary>
        public QuestionnaireDocument Document { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// The get questionnaire document.
        /// </summary>
        /// <returns>
        /// The <see cref="QuestionnaireDocument"/>.
        /// </returns>
        private QuestionnaireDocument GenerateQuestionnaireDocument()
        {
            var doc = new QuestionnaireDocument();
            var mainGroup = new Group("Main") { PublicKey = this.MainGroupKey };
            var group1 = new Group();
            var question1 = new SingleQuestion(this.QuestionKey, "Q1");
            var answer1 = new Answer { PublicKey = this.Answer1Key, AnswerValue = "1" };
            var answer2 = new Answer { PublicKey = this.Answer2Key, AnswerValue = "2" };

            var question2Prop = new AutoPropagateQuestion("Q1")
                {
                   PublicKey = this.AutoPropQuestionKey, MaxValue = 10, Triggers = new List<Guid> { this.AutoGroupKey } 
                };

            question1.AddAnswer(answer1);
            question1.AddAnswer(answer2);
            group1.Children.Add(question1);
            group1.Children.Add(question2Prop);

            var group2Auto = new Group { Propagated = Propagate.AutoPropagated, PublicKey = this.AutoGroupKey };

            var autoQuestion1 = new SingleQuestion(this.AutoQuestionKey, "Q1a");
            var autoAnswer1 = new Answer { PublicKey = this.AutoAnswer1Key, AnswerValue = "1" };
            var autoAnswer2 = new Answer { PublicKey = this.AutoAnswer2Key, AnswerValue = "2" };

            autoQuestion1.AddAnswer(autoAnswer1);
            autoQuestion1.AddAnswer(autoAnswer2);
            group2Auto.Children.Add(autoQuestion1);

            mainGroup.Children.Add(group1);
            mainGroup.Children.Add(group2Auto);
            doc.Add(mainGroup, null, null);

            return doc;
        }

        #endregion
    }
}