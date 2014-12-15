using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class DownloadQuestionnaireRequest
    {
        public Guid QuestionnaireId { get; set; }
        public QuestionnaireVersion SupportedQuestionnaireVersion { get; set; }
    }
}
