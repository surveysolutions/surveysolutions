// -----------------------------------------------------------------------
// <copyright file="AutoPropagateQuestionView.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.View.Question
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Question view for autopropagate
    /// </summary>
    public class AutoPropagateQuestionView : BaseQuestionView
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoPropagateQuestionView"/> class.
        /// </summary>
        /// <param name="view">
        /// The view.
        /// </param>
        public AutoPropagateQuestionView(QuestionView view)
            : base(view)
        {
            this.MaxValue = view.MaxValue;
            this.Triggers = view.Triggers;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Triggers.
        /// </summary>
        public List<Guid> Triggers { get; set; }

        /// <summary>
        /// Gets or sets MaxValue
        /// </summary>
        [Range(0, 2147483647, ErrorMessage = "This number must be between 0 and 2147483647")]
        public int MaxValue { get; set; }

        #endregion
    }
}
