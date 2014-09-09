using System;
using System.ServiceModel;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.UI.Designer.Services.Questionnaire
{
    /// <summary>
    ///     The questionnaire browse item.
    /// </summary>
    [MessageContract]
    public class QuestionnaireListViewItemMessage
    {
        public QuestionnaireListViewItemMessage() { }
        public QuestionnaireListViewItemMessage(QuestionnaireListViewItem data)
        {
            this.Id = data.PublicId;
            this.Title = data.Title;
        }
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