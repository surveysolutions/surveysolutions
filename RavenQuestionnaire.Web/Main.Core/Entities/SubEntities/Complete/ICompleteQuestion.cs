// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompleteQuestion.cs" company="">
//   
// </copyright>
// <summary>
//   The CompleteQuestion interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities.Complete
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The CompleteQuestion interface.
    /// </summary>
    public interface ICompleteQuestion : IQuestion
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the answer date.
        /// </summary>
        DateTime? AnswerDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the propogation public key.
        /// </summary>
        Guid? PropogationPublicKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether valid.
        /// </summary>
        bool Valid { get; set; }

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
        /// The comments.
        /// </param>
        void SetComments(string comments);

        #endregion
    }
}