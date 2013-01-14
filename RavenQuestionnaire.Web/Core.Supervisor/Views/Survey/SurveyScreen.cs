// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyScreen.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   The survey screen.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Utility;
    using Main.Core.View.Question;

    /// <summary>
    /// The survey screen.
    /// </summary>
    public class SurveyScreen
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyScreen"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="node">
        /// The group.
        /// </param>
        public SurveyScreen(CompleteQuestionnaireStoreDocument doc, NodeWithLevel node)
        {
            this.Key = node.Group.GetKey();

            this.Title = node.Group.Title;

            this.Questions = node.Group.Children.OfType<ICompleteQuestion>()
                .Where(q => q.QuestionScope <= QuestionScope.Supervisor)
                .Select(q => new CompleteQuestionView(doc, q))
                .ToList();
        }

        #endregion

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public ScreenKey Key { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        public List<CompleteQuestionView> Questions { get; set; }
    }
}