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
        /// Gets or sets the validated time.
        /// </summary>
        DateTime ValidatedTime { get; set; }

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

        /// <summary>
        /// The get answer object.
        /// </summary>
        /// <returns>
        /// The System.Object.
        /// </returns>
        object GetAnswerObject();

        /// <summary>
        /// The is answered.
        /// </summary>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        bool IsAnswered();

        /// <summary>
        /// The get answer string.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        string GetAnswerString();

        /// <summary>
        /// The set answer.
        /// </summary>
        /// <param name="answerKeys">
        /// The answer keys.
        /// </param>
        /// <param name="answerValue">
        /// The answer value.
        /// </param>
        void SetAnswer(List<Guid> answerKeys, string answerValue);

        /// <summary>
        /// The set comments.
        /// </summary>
        /// <param name="comments">
        ///   The comments.
        /// </param>
        /// <param name="date">
        /// The date
        /// </param>
        /// <param name="user">
        /// The user
        /// </param>
        void SetComments(string comments, DateTime date, UserLight user);
        
       #endregion
    }
}