// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The abstract questionnaire view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.Core.View.Question;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;

    using RavenQuestionnaire.Core.Views.Group;
    using RavenQuestionnaire.Core.Views.Question;

    /// <summary>
    /// The abstract questionnaire view.
    /// </summary>
    public abstract class AbstractQuestionnaireView
    {
        #region Fields

        /// <summary>
        /// The public key.
        /// </summary>
        public Guid PublicKey;

        #endregion

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
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }

    /// <summary>
    /// The abstract questionnaire view.
    /// </summary>
    /// <typeparam name="TGroup">
    /// </typeparam>
    /// <typeparam name="TQuestion">
    /// </typeparam>
    public abstract class AbstractQuestionnaireView<TGroup, TQuestion> : AbstractQuestionnaireView
        where TGroup : AbstractGroupView where TQuestion : AbstractQuestionView, ICompositeView
    {
        #region Fields

        /// <summary>
        /// The questions.
        /// </summary>
        private TQuestion[] questions;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionnaireView{TGroup,TQuestion}"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public AbstractQuestionnaireView(IQuestionnaireDocument doc)
        {
            this.Children = new List<ICompositeView>();
            this.Questions = new TQuestion[0];
            this.Groups = new TGroup[0];
            this.PublicKey = doc.PublicKey;
            this.Title = doc.Title;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractQuestionnaireView{TGroup,TQuestion}"/> class.
        /// </summary>
        public AbstractQuestionnaireView()
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
        /// Gets or sets the groups.
        /// </summary>
        public TGroup[] Groups { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        public Guid? Parent { get; set; }

        /// <summary>
        /// Gets or sets the questions.
        /// </summary>
        public TQuestion[] Questions
        {
            get
            {
                return this.questions;
            }

            set
            {
                this.questions = value;
                for (int i = 0; i < this.questions.Length; i++)
                {
                    this.questions[i].Index = i + 1;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// The questionnaire view.
    /// </summary>
    public class QuestionnaireView : AbstractQuestionnaireView<GroupView, QuestionView>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireView"/> class.
        /// </summary>
        public QuestionnaireView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        public QuestionnaireView(IQuestionnaireDocument doc)
            : base(doc)
        {
            foreach (IComposite composite in doc.Children)
            {
                if ((composite as IQuestion) != null)
                {
                    var q = composite as IQuestion;
                    List<IQuestion> r = doc.Children.OfType<IQuestion>().ToList();
                    this.Children.Add(new QuestionView(doc, q) { Index = r.IndexOf(q) });
                }
                else
                {
                    var g = composite as IGroup;
                    this.Children.Add(new GroupView(doc, g));
                }
            }

            this.Questions = doc.Children.OfType<IQuestion>().Select(q => new QuestionView(doc, q)).ToArray();
            this.Groups = doc.Children.OfType<IGroup>().Select(g => new GroupView(doc, g)).ToArray();
        }

        #endregion
    }
}