using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    [Obsolete("remove after all HQ will be 5.11+")]
    public class DownloadQuestionnaireRequest
    {
        public Guid QuestionnaireId { get; set; }
        public QuestionnaireVersion SupportedVersion { get; set; }
    }
}
