namespace Main.Core.Entities.Extensions
{
    using System;

    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The complete question wrapper.
    /// </summary>
    public class CompleteQuestionWrapper
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionWrapper"/> class.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="screenGuid">
        /// The screen guid.
        /// </param>
        public CompleteQuestionWrapper(ICompleteQuestion question, Guid screenGuid)
        {
            this.Question = question;
            this.GroupKey = screenGuid;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the group key.
        /// </summary>
        public Guid GroupKey { get; private set; }

        /// <summary>
        /// Gets the question.
        /// </summary>
        public ICompleteQuestion Question { get; private set; }

        #endregion
    }
}
