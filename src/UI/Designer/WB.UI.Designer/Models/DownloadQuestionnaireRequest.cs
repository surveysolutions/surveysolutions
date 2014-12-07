using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Designer.Models
{
    public class DownloadQuestionnaireRequest
    {
        public Guid QuestionnaireId { get; set; }
        public QuestionnaireVersion SupportedQuestionnaireVersion { get; set; }
    }
}
