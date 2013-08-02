namespace Main.Core.Entities.SubEntities.Complete
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Documents;

    /// <summary>
    /// The CompleteQuestion interface.
    /// </summary>
    public interface ICompleteQuestion : IQuestion, ICompleteItem
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether valid.
        /// </summary>
        bool Valid { get; set; }

        /// <summary>
        /// Gets or sets the answer date.
        /// </summary>
        DateTime? AnswerDate { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        string LastComment { get; set; }

        /// <summary>
        /// Gets or sets comments.
        /// </summary>
        List<CommentDocument> Comments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsFlaged.
        /// </summary>
        bool IsFlaged { get; set; }

        #endregion

        #region Public Methods and Operators

        object GetAnswerObject();

        bool IsAnswered();

        string GetAnswerString();

        void SetAnswer(List<Guid> answerKeys, string answerValue);

        void SetComments(string comments, DateTime date, UserLight user);

        void ThrowDomainExceptionIfAnswerInvalid(List<Guid> answerKeys, string answerValue);

        #endregion
    }
}