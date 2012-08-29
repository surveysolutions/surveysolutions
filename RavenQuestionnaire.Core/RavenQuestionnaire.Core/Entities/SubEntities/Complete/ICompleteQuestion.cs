// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompleteQuestion.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The CompleteQuestion interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
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

    /// <summary>
    /// The NumericQuestion interface.
    /// </summary>
    public interface INumericQuestion
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the add numeric attr.
        /// </summary>
        string AddNumericAttr { get; set; }

        /// <summary>
        /// Gets or sets the int attr.
        /// </summary>
        int IntAttr { get; set; }

        #endregion
    }

    /// <summary>
    /// The SingleQuestion interface.
    /// </summary>
    public interface ISingleQuestion
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the add single attr.
        /// </summary>
        string AddSingleAttr { get; set; }

        #endregion
    }

    /// <summary>
    /// The DateTimeQuestion interface.
    /// </summary>
    public interface IDateTimeQuestion
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the add date time attr.
        /// </summary>
        string AddDateTimeAttr { get; set; }

        /// <summary>
        /// Gets or sets the date time attr.
        /// </summary>
        DateTime DateTimeAttr { get; set; }

        #endregion
    }

    /// <summary>
    /// The MultyOptionsQuestion interface.
    /// </summary>
    public interface IMultyOptionsQuestion
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the add multy attr.
        /// </summary>
        string AddMultyAttr { get; set; }

        #endregion
    }

    /// <summary>
    /// The GpsCoordinatesQuestion interface.
    /// </summary>
    public interface IGpsCoordinatesQuestion
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the add gps coordinate attr.
        /// </summary>
        string AddGpsCoordinateAttr { get; set; }

        /// <summary>
        /// Gets or sets the int attr.
        /// </summary>
        char IntAttr { get; set; }

        #endregion
    }

    /// <summary>
    /// The PercentageQuestion interface.
    /// </summary>
    public interface IPercentageQuestion
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the add percentage attr.
        /// </summary>
        double AddPercentageAttr { get; set; }

        #endregion
    }

    /// <summary>
    /// The TextCompleteQuestion interface.
    /// </summary>
    public interface ITextCompleteQuestion
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the add text attr.
        /// </summary>
        string AddTextAttr { get; set; }

        #endregion
    }

    /// <summary>
    /// The AutoPropagate interface.
    /// </summary>
    public interface IAutoPropagate
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the target group key.
        /// </summary>
        Guid TargetGroupKey { get; set; }

        #endregion
    }
}