using System;
using System.ServiceModel;

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
        public Version SupportedExpressionsEngineVersion { get; set; }
    }
}