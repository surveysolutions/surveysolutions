namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Extensions;
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

            this.Captions = new Dictionary<string, string>();
            if (node.Group.PropagationPublicKey.HasValue)
            {
                this.Captions.Add(
                    node.Group.PropagationPublicKey.Value.ToString(),
                    string.Concat(
                        doc.GetPropagatedGroupsByKey(node.Group.PropagationPublicKey.Value).SelectMany(q => q.Children)
                    .OfType<ICompleteQuestion>()
                    .Where(q => q.Capital)
                    .Select(q => q.GetAnswerString() + " ")).Trim());
            }

            this.Questions = node.Group.Children.OfType<ICompleteQuestion>()
                .Where(q => q.QuestionScope <= QuestionScope.Supervisor)
                .Select(q => new SurveyQuestion(doc, node.Group, q))
                .ToList();

            this.ChildrenKeys = node.Group.Children.OfType<ICompleteGroup>().Select(q => new ScreenKey(q)).ToList();
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

        /// <summary>
        /// Gets or sets Questions.
        /// </summary>
        public List<SurveyQuestion> Questions { get; set; }

        /// <summary>
        /// Gets or sets Captions.
        /// </summary>
        public Dictionary<string, string> Captions { get; set; }

        /// <summary>
        /// Gets or sets ChildrenKeys.
        /// </summary>
        public List<ScreenKey> ChildrenKeys { get; set; }
    }
}