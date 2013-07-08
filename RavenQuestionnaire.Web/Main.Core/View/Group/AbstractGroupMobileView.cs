namespace Main.Core.View.Group
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;
    using Main.Core.View.Question;

    /// <summary>
    /// The abstract group mobile view.
    /// </summary>
    public abstract class AbstractGroupMobileView : ICompositeView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractGroupMobileView"/> class.
        /// </summary>
        public AbstractGroupMobileView()
        {
            this.Children = new List<ICompositeView>();
            this.QuestionsWithCards = new List<CompleteQuestionView>();
            this.QuestionsWithInstructions = new List<CompleteQuestionView>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<ICompositeView> Children { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }


        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        public Guid? Parent { get; set; }
        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the propagated.
        /// </summary>
        public Propagate Propagated { get; set; }
        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }
        /// <summary>
        /// get or set questionnaire active status - active if allow to edit, not error or completed
        /// </summary>
        public bool IsQuestionnaireActive { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire public key.
        /// </summary>
        public Guid QuestionnairePublicKey { get; set; }

        /// <summary>
        /// Gets or sets the questions with cards.
        /// </summary>
        public List<CompleteQuestionView> QuestionsWithCards { get; set; }

        /// <summary>
        /// Gets or sets the questions with instructions.
        /// </summary>
        public List<CompleteQuestionView> QuestionsWithInstructions { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        public ICompositeView ParentView { get; private set; }

        /// <summary>
        /// Gets or sets the unique key.
        /// </summary>
        public Guid UniqueKey { get; set; }

        #endregion
    }
}