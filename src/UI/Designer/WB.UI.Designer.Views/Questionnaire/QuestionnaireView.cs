namespace WB.UI.Designer.Views.Questionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;

    /// <summary>
    /// The questionnaire view.
    /// </summary>
    public class QuestionnaireView
    {
        #region Fields

        /// <summary>
        /// The children.
        /// </summary>
        private IEnumerable<ICompositeView> children;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public QuestionnaireView(IQuestionnaireDocument doc)
        {
            this.Source = doc;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the children.
        /// </summary>
        public IEnumerable<ICompositeView> Children
        {
            get
            {
                return this.children
                       ?? (this.children =
                           this.Source.Children.Cast<IGroup>().Select(@group => new GroupView(this.Source, @group)).ToList());
            }
        }

        /// <summary>
        /// Gets the created by.
        /// </summary>
        public Guid? CreatedBy
        {
            get
            {
                return this.Source.CreatedBy;
            }
        }

        /// <summary>
        /// Gets the creation date.
        /// </summary>
        public DateTime CreationDate
        {
            get
            {
                return this.Source.CreationDate;
            }
        }

        /// <summary>
        /// Gets the last entry date.
        /// </summary>
        public DateTime LastEntryDate
        {
            get
            {
                return this.Source.LastEntryDate;
            }
        }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        public Guid? Parent { get; set; }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        public Guid PublicKey
        {
            get
            {
                return this.Source.PublicKey;
            }
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        public IQuestionnaireDocument Source { get; private set; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title
        {
            get
            {
                return this.Source.Title;
            }
        }

        /// <summary>
        /// Gets the is public.
        /// </summary>
        public bool IsPublic
        {
            get
            {
                return this.Source.IsPublic;
            }
        }

        #endregion
    }
}