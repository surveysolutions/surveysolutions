// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireListViewItem.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer.WebServices.Questionnaire
{
    using System;
    using System.ServiceModel;

    /// <summary>
    ///     The questionnaire browse item.
    /// </summary>
    [MessageContract]
    public class QuestionnaireListViewItem
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [MessageHeader]
        public Guid Id;

        /// <summary>
        ///     Gets or sets the title.
        /// </summary>
        [MessageHeader]
        public string Title;

        #endregion
    }
}