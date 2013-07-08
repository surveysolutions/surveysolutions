using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Answer;

namespace Core.CAPI.Views.Json
{
    /// <summary>
    /// The complete questions json view.
    /// </summary>
    public class CompleteQuestionsJsonView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionsJsonView"/> class.
        /// </summary>
        public CompleteQuestionsJsonView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionsJsonView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <param name="isInPropagatebleGroup">
        /// The is in propagateble group.
        /// </param>
        public CompleteQuestionsJsonView(ICompleteQuestion doc, Guid groupPublicKey, bool isInPropagatebleGroup)
        {
            this.IsInPropagatebleGroup = isInPropagatebleGroup;
            this.GroupPublicKey = groupPublicKey;
            this.Title = doc.QuestionText;
            this.QuestionType = doc.QuestionType;
            this.PublicKey = doc.PublicKey;
            this.Featured = doc.Featured;
            this.Valid = doc.Valid;
            this.Comments = doc.LastComment;
            this.Enabled = doc.Enabled;
            this.Mandatory = doc.Mandatory;
            CompleteAnswerView[] answers =
                doc.Answers.OfType<ICompleteAnswer>().Select(a => new CompleteAnswerView(doc.PublicKey, a)).ToArray();
            this.Answer = doc.GetAnswerString();
            this.Answered = doc.IsAnswered();
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
        /// Gets or sets the comments.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether featured.
        /// </summary>
        public bool Featured { get; set; }

        /// <summary>
        /// Gets or sets the group public key.
        /// </summary>
        public Guid GroupPublicKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is in propagateble group.
        /// </summary>
        public bool IsInPropagatebleGroup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mandatory.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question type.
        /// </summary>
        public QuestionType QuestionType { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether valid.
        /// </summary>
        public bool Valid { get; set; }

        #endregion
    }
}