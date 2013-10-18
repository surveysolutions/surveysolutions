using System.Linq;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.UI.Designer.WebServices.Questionnaire
{
    using System.ServiceModel;

    /// <summary>
    ///     The questionnaire browse view.
    /// </summary>
    [MessageContract]
    public class QuestionnaireListViewMessage
    {
       // public QuestionnaireListViewMessage() {}

        public QuestionnaireListViewMessage(QuestionnaireListView data)
        {
            this.Order = data.Order;
            this.Page = data.Page;
            this.PageSize = data.PageSize;
            this.TotalCount = data.TotalCount;
            this.Items = data.Items.Select(item => new QuestionnaireListViewItemMessage(item)).ToArray();
        }

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
        public QuestionnaireListViewItemMessage[] Items;

        #endregion
    }
}