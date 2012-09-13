// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BindedCompleteQuestionView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The binded complete question view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Question
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities.Complete;

    using RavenQuestionnaire.Core.Views.Answer;

    /// <summary>
    /// The binded complete question view.
    /// </summary>
    public class BindedCompleteQuestionView : CompleteQuestionView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BindedCompleteQuestionView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public BindedCompleteQuestionView(
            CompleteQuestionnaireDocument doc, ICompleteGroup group, ICompleteQuestion question)
        {
            this.Enabled = false;
            var bindedQuestion = question as IBinded;
            if (bindedQuestion == null)
            {
                throw new ArgumentException();
            }

            IEnumerable<AbstractCompleteQuestion> templates =
                doc.Find<AbstractCompleteQuestion>(q => q.PublicKey == bindedQuestion.ParentPublicKey);
            AbstractCompleteQuestion template = templates.FirstOrDefault();
            if (templates.Count() > 1)
            {
                // IPropogate propagatebleGroup = group as IPropogate;
                if (!group.PropogationPublicKey.HasValue)
                {
                    return;
                }

                // var questionnaire = new Main.Core.Entities.CompleteQuestionnaire(doc);
                template =
                    doc.Find<ICompleteGroup>(g => g.PropogationPublicKey == group.PropogationPublicKey.Value).SelectMany
                        (g => g.Find<AbstractCompleteQuestion>(q => q.PublicKey.Equals(bindedQuestion.ParentPublicKey)))
                        .FirstOrDefault();
                if (template == null)
                {
                    return;
                }
            }

            this.Answers =
                template.Children.OfType<ICompleteAnswer>().Select(a => new CompleteAnswerView(template.PublicKey, a)).
                    ToArray();
            this.PublicKey = template.PublicKey;
            this.Title = template.QuestionText;
            this.Instructions = template.Instructions;
            this.Comments = template.Comments;
            this.ConditionExpression = template.ConditionExpression;
            this.StataExportCaption = template.StataExportCaption;
            this.Parent = group.PublicKey;

            // thi's.QuestionnaireId = group.
        }

        #endregion
    }
}