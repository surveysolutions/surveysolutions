namespace Main.Core.View.Question
{
    using System;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// Class for creation base question view
    /// </summary>
    public class BaseQuestionView
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseQuestionView"/> class.
        /// </summary>
        /// <param name="view">
        /// The view.
        /// </param>
        public BaseQuestionView(QuestionView view)
        {
            this.Comments = view.Comments;
            this.ConditionExpression = view.ConditionExpression;
            this.Featured = view.Featured;
            this.Instructions = view.Instructions;
            this.Mandatory = view.Mandatory;
            this.Capital = view.Capital;
            this.Parent = view.Parent.HasValue ? view.Parent.Value : Guid.Empty;
            this.PublicKey = view.PublicKey;
            this.QuestionScope = view.QuestionScope;
            this.QuestionType = view.QuestionType;
            this.QuestionnaireKey = view.QuestionnaireKey;
            this.StataExportCaption = view.StataExportCaption;
            this.Title = view.Title;
            this.ValidationExpression = view.ValidationExpression;
            this.ValidationMessage = view.ValidationMessage;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets PublicKey.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets Parent.
        /// </summary>
        public Guid Parent { get; set; }

        /// <summary>
        /// Gets or sets QuestionnaireKey.
        /// </summary>
        public Guid QuestionnaireKey { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets QuestionType.
        /// </summary>
        public QuestionType QuestionType { get; set; }

        /// <summary>
        /// Gets or sets question scope.
        /// </summary>
        public QuestionScope QuestionScope { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Featured.
        /// </summary>
        public bool Featured { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Mandatory.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Capital.
        /// </summary>
        public bool Capital { get; set; }

        /// <summary>
        /// Gets or sets StataExportCaption.
        /// </summary>
        public string StataExportCaption { get; set; }

        /// <summary>
        /// Gets or sets ConditionExpression.
        /// </summary>
        public string ConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets ValidationExpression.
        /// </summary>
        public string ValidationExpression { get; set; }

        /// <summary>
        /// Gets or sets ValidationMessage.
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// Gets or sets Instructions.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// Gets or sets Comments.
        /// </summary>
        public string Comments { get; set; }

        #endregion
    }
}
