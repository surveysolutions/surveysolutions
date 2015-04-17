using System;
using System.ServiceModel;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions;

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
        public ExpressionsEngineVersion SupportedExpressionsEngineVersion { get; set; }
    }
}