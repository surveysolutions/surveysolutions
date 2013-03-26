// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The abstract questionnaire view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View.Question;

    using Group;

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
        }

        #endregion
    }
}