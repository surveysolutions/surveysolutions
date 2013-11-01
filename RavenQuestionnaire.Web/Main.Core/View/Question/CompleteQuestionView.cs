using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Answer;

namespace Main.Core.View.Question
{
    /// <summary>
    /// The complete question view.
    /// </summary>
    public class CompleteQuestionView 
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
        public CompleteQuestionView(ICompleteQuestionnaireDocument questionnaire, ICompleteQuestion doc)
        {
            this.Valid = doc.Valid;
            this.Enabled = doc.Enabled;
            this.Answer = doc.GetAnswerString();
            this.Answered = doc.IsAnswered();
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
        /// Gets or sets a value indicating whether Editable.
        /// </summary>
        public bool Editable { get; set; }

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