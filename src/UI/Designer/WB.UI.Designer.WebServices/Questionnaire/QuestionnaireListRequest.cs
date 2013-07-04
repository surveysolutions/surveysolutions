namespace WB.UI.Designer.WebServices.Questionnaire
{
    using System.ServiceModel;

    /// <summary>
    /// The questionnaire list request.
    /// </summary>
    [MessageContract]
    public class QuestionnaireListRequest
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        [MessageHeader]
        public string Filter;

        /// <summary>
        /// Gets or sets the page index.
        /// </summary>
        [MessageHeader]
        public int PageIndex;

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        [MessageHeader]
        public int PageSize;

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        [MessageHeader]
        public string SortOrder;

        #endregion
    }
}