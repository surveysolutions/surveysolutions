namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// Survey Question
    /// </summary>
    public class SurveyQuestion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyQuestion"/> class.
        /// </summary>
        /// <param name="doc">
        /// The survey
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="question">
        /// The question.
        /// </param>
        public SurveyQuestion(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group, ICompleteQuestion question)
        {
            this.SurveyPublicKey = doc.PublicKey;
            this.Featured = question.Featured;
            this.Mandatory = question.Mandatory;
            this.Capital = question.Capital;
            this.PublicKey = question.PublicKey;
            this.Title = question.QuestionText;
            this.Answers = new List<SurveyAnswer> { new SurveyAnswer(group, question) };
        }

        /// <summary>
        /// Gets or sets a value indicating whether Featured.
        /// </summary>
        public bool Featured { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Mandatory.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets PublicKey.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets SurveyPublicKey.
        /// </summary>
        public Guid SurveyPublicKey { get; set; }

        /// <summary>
        /// Gets or sets Answers.
        /// </summary>
        public List<SurveyAnswer> Answers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Capital.
        /// </summary>
        public bool Capital { get; set; }
    }
}
