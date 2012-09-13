// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropagatedGroup.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The propagated group.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Views.Answer;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
    using RavenQuestionnaire.Core.Views.Question;

    /// <summary>
    /// The propagated group.
    /// </summary>
    public class PropagatedGroup
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropagatedGroup"/> class.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="isAutoPropagate">
        /// The is auto propagate.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <param name="questions">
        /// The questions.
        /// </param>
        public PropagatedGroup(
            Guid key, string text, bool isAutoPropagate, Guid propagationKey, List<CompleteQuestionView> questions)
        {
            this.PublicKey = key;
            this.Title = text;
            this.AutoPropagate = isAutoPropagate;
            this.PropogationKey = propagationKey;
            this.Questions = questions;
            this.Navigation = new ScreenNavigation();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether auto propagate.
        /// </summary>
        public bool AutoPropagate { get; private set; }

        /// <summary>
        /// Gets or sets the featured title.
        /// </summary>
        public string FeaturedTitle { get; set; }

        /// <summary>
        /// Gets the first answer.
        /// </summary>
        public string FirstAnswer
        {
            get
            {
                List<CompleteAnswerView> answers = this.Questions.First().Answers.Where(a => a.Selected).ToList();
                string firstAnswer;

                switch (answers.Count)
                {
                    case 0:
                        firstAnswer = "Answer the first question";
                        break;
                    case 1:
                        {
                            CompleteAnswerView answer = answers[0];
                            if (!string.IsNullOrEmpty(answer.AnswerValue))
                            {
                                firstAnswer = answer.AnswerValue;
                            }
                            else if (!string.IsNullOrEmpty(answer.Title))
                            {
                                firstAnswer = answer.Title;
                            }
                            else
                            {
                                firstAnswer = "Fill parcel name field";
                            }
                        }

                        break;
                    default:
                        firstAnswer = "Multiple answers";
                        break;
                }

                if (string.IsNullOrWhiteSpace(firstAnswer))
                {
                    firstAnswer = "Answer the first question";
                }

                return firstAnswer;
            }
        }

        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        public ScreenNavigation Navigation { get; set; }

        /// <summary>
        /// Gets the propogation key.
        /// </summary>
        public Guid PropogationKey { get; private set; }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        public Guid PublicKey { get; private set; }

        /// <summary>
        /// Gets the questions.
        /// </summary>
        public List<CompleteQuestionView> Questions { get; private set; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; private set; }

        #endregion
    }
}