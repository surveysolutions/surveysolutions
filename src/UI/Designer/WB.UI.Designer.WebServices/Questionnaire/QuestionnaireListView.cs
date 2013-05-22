// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireListView.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer.WebServices.Questionnaire
{
    using System.ServiceModel;

    /// <summary>
    ///     The questionnaire browse view.
    /// </summary>
    [MessageContract]
    public class QuestionnaireListView
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the order.
        /// </summary>
        [MessageHeader]
        public string Order;

        /// <summary>
        ///     Gets the page.
        /// </summary>
        [MessageHeader]
        public int Page;

        /// <summary>
        ///     Gets the page size.
        /// </summary>
        [MessageHeader]
        public int PageSize;

        /// <summary>
        ///     Gets the total count.
        /// </summary>
        [MessageHeader]
        public int TotalCount;

        /// <summary>
        ///     Gets the items.
        /// </summary>
        [MessageBodyMember]
        public QuestionnaireListViewItem[] Items;

        #endregion
    }
}