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
    using Main.Core.View.Group;

    /// <summary>
    /// The complete group headers.
    /// </summary>
    public class DetailsMenuItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailsMenuItem"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc
        /// </param>
        /// <param name="node">
        /// The node.
        /// </param>
        public DetailsMenuItem(CompleteQuestionnaireStoreDocument doc, NodeWithLevel node)
        {
            var group = node.Group;
            this.Key = new ScreenKey(group);
            this.PublicKey = group.PublicKey;
            this.GroupText = group.PropagationPublicKey.HasValue
                                 ? string.Concat(
                                     doc.GetPropagatedGroupsByKey(group.PropagationPublicKey.Value).SelectMany(
                                         q => q.Children).OfType<ICompleteQuestion>().Where(q => q.Capital).Select(
                                             q => q.GetAnswerString() + " ")) + " " + group.Title
                                 : group.Title;

            this.PropagationKey = group.PropagationPublicKey;
            this.Enabled = group.Enabled;
            this.Description = group.Description;
            this.Totals = this.CalcProgress(group);
            this.Propagated = group.Propagated;
            this.Level = node.Level;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailsMenuItem"/> class.
        /// </summary>
        public DetailsMenuItem()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the group text.
        /// </summary>
        public string GroupText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is current.
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Gets or sets the propagation key.
        /// </summary>
        public Guid? PropagationKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the totals.
        /// </summary>
        public Counter Totals { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets Propagated.
        /// </summary>
        public Propagate Propagated { get; set; }

        /// <summary>
        /// Gets or sets Level.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets Key.
        /// </summary>
        public ScreenKey Key { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The calc progress.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Counter.
        /// </returns>
        private Counter CalcProgress(ICompleteGroup @group)
        {
            var total = new Counter();
            List<ICompleteGroup> gruoSubGroup = @group.Children.OfType<ICompleteGroup>().ToList();
            List<ICompleteQuestion> gruoSubQuestions = @group.Children.OfType<ICompleteQuestion>().ToList();
            if (@group.PropagationPublicKey.HasValue)
            {
                total = total + this.CountQuestions(gruoSubQuestions);
                return total;
            }

            var complete = @group as CompleteGroup;
            if (complete != null && complete.Propagated != Propagate.None)
            {
                return total;
            }


            total = total + this.CountQuestions(gruoSubQuestions);
            foreach (ICompleteGroup g in gruoSubGroup)
            {
                total = total + this.CalcProgress(g);
            }

            return total;
        }

        /// <summary>
        /// The count questions.
        /// </summary>
        /// <param name="questions">
        /// The questions.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Counter.
        /// </returns>
        private Counter CountQuestions(List<ICompleteQuestion> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                return new Counter();
            }

            List<ICompleteQuestion> enabled = questions.Where(q => q.Enabled).ToList();
            var total = new Counter
            {
                Total = questions.Count,
                Enablad = enabled.Count(),
                Answered = enabled.Count(question => question.GetAnswerObject() != null)
            };
            return total;
        }

        #endregion
    }
}