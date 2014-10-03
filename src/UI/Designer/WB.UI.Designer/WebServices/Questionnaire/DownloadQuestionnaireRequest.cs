using System;
using System.ServiceModel;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.UI.Designer.WebServices.Questionnaire
{
    /// <summary>
    /// The download questionnaire request.
    /// </summary>
    [MessageContract]
    public class DownloadQuestionnaireRequest
    {
        [MessageHeader]
        public Guid QuestionnaireId { get; set; }

        [MessageHeader]
        public QuestionnaireVersion SupportedQuestionnaireVersion { get; set; }
    }
}