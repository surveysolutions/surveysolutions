namespace WB.UI.Designer.WebServices.Questionnaire
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// The download questionnaire request.
    /// </summary>
    [MessageContract]
    public class DownloadQuestionnaireRequest
    {
        #region Fields

        /// <summary>
        /// The file name.
        /// </summary>
        [MessageHeader]
        public Guid QuestionnaireId { get; set; }

        #endregion
    }
}