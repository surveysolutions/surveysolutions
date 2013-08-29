using System;

namespace Main.Core.View.CompleteQuestionnaire
{
    /// <summary>
    /// The complete questionnaire view input model.
    /// </summary>
    public class CompleteQuestionnaireViewInputModel
    {
        #region Fields

        /// <summary>
        /// The _current group public key.
        /// </summary>
        private Guid? _currentGroupPublicKey;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireViewInputModel"/> class.
        /// </summary>
        public CompleteQuestionnaireViewInputModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireViewInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public CompleteQuestionnaireViewInputModel(Guid id)
        {
            this.CompleteQuestionnaireId = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireViewInputModel"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="groupKey">
        /// The group key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        public CompleteQuestionnaireViewInputModel(Guid id, Guid groupKey, Guid? propagationKey)
        {
            this.CompleteQuestionnaireId = id;
            this.CurrentGroupPublicKey = groupKey;
            this.PropagationKey = propagationKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the complete questionnaire id.
        /// </summary>
        public Guid CompleteQuestionnaireId { get; private set; }

        /// <summary>
        /// Gets or sets the current group public key.
        /// </summary>
        public Guid? CurrentGroupPublicKey
        {
            get
            {
                return this._currentGroupPublicKey;
            }

            set
            {
                this._currentGroupPublicKey = value == Guid.Empty ? null : value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is reverse.
        /// </summary>
        public bool IsReverse { get; private set; }

        /// <summary>
        /// Gets or sets the propagation key.
        /// </summary>
        public Guid? PropagationKey { get; set; }

        #endregion

        // public Guid? CurrentScreenPublicKey { get; set; }

        public Entities.SubEntities.QuestionScope Scope { get; set; }
    }
}