// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete question view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Question
{
    using System;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    using RavenQuestionnaire.Core.Views.Answer;
    using RavenQuestionnaire.Core.Views.Card;

    /// <summary>
    /// The complete question view.
    /// </summary>
    public class CompleteQuestionView : QuestionView<CompleteAnswerView, ICompleteGroup, ICompleteQuestion>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionView"/> class.
        /// </summary>
        public CompleteQuestionView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionView"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        public CompleteQuestionView(string questionnaireId, Guid? groupPublicKey)
            : base(questionnaireId, groupPublicKey)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionView"/> class.
        /// </summary>
        /// <param name="questionnaire">
        /// The questionnaire.
        /// </param>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public CompleteQuestionView(CompleteQuestionnaireStoreDocument questionnaire, ICompleteQuestion doc)
            : base(questionnaire, doc)
        {
            this.QuestionnaireKey = questionnaire.PublicKey;
            this.Valid = doc.Valid;
            this.Enabled = doc.Enabled;
            this.Answers =
                doc.Children.OfType<ICompleteAnswer>().Select(a => new CompleteAnswerView(doc.PublicKey, a)).ToArray();
            this.Answer = doc.GetAnswerString();
            this.Answered = doc.GetAnswerObject() != null;
            this.Featured = doc.Featured;
            this.Mandatory = doc.Mandatory;
            this.Comments = doc.Comments;
            if (doc.Cards != null)
            {
                this.Cards =
                    doc.Cards.Select(card => new CardView(doc.PublicKey, card)).OrderBy(a => Guid.NewGuid()).ToArray();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionView"/> class.
        /// </summary>
        /// <param name="questionnaire">
        /// The questionnaire.
        /// </param>
        /// <param name="doc">
        /// The doc.
        /// </param>
        protected CompleteQuestionView(IQuestionnaireDocument questionnaire, IQuestion doc)
            : base(questionnaire, doc)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the answer.
        /// </summary>
        public object Answer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether answered.
        /// </summary>
        public bool Answered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the parent group public key.
        /// </summary>
        public Guid ParentGroupPublicKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether valid.
        /// </summary>
        public bool Valid { get; set; }

        #endregion
    }
}