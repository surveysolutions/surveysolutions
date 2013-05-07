// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DownloadQuestionnaireRequest.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.WebServices
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
        [MessageBodyMember]
        public Guid QuestionnaireId;

        #endregion
    }
}