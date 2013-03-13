namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;
    using Main.Core.View;
    using Main.Core.View.Question;

    using RavenQuestionnaire.Core.Views.Group;

    /// <summary>
    /// The abstract questionnaire view.
    /// </summary>
    public abstract class AbstractQuestionnaireView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionnaireView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        protected AbstractQuestionnaireView(IQuestionnaireDocument doc)
            : this()
        {
            this.PublicKey = doc.PublicKey;
            this.Title = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionnaireView"/> class.
        /// </summary>
        protected AbstractQuestionnaireView()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the last entry date.
        /// </summary>
        public DateTime LastEntryDate { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }

    /// <summary>
    /// The abstract questionnaire view.
    /// </summary>
    /// <typeparam name="TGroup">
    /// Group type
    /// </typeparam>
    /// <typeparam name="TQuestion">
    /// Question type
    /// </typeparam>
    public abstract class AbstractQuestionnaireView<TGroup, TQuestion> : AbstractQuestionnaireView
        where TGroup : AbstractGroupView where TQuestion : AbstractQuestionView, ICompositeView
    {
        #region Constants and Fields

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionnaireView{TGroup,TQuestion}"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        protected AbstractQuestionnaireView(IQuestionnaireDocument doc)
            : base(doc)
        {
            this.Children = new List<ICompositeView>();
            this.PublicKey = doc.PublicKey;
            this.Title = doc.Title;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionnaireView{TGroup,TQuestion}"/> class.
        /// </summary>
        protected AbstractQuestionnaireView()
        {
            this.Children = new List<ICompositeView>();
        }

        #endregion

        // public Guid PublicKey { get; set; }
        // public string Title { get; set; }
        #region Public Properties

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<ICompositeView> Children { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        public Guid? Parent { get; set; }

        #endregion
    }
}