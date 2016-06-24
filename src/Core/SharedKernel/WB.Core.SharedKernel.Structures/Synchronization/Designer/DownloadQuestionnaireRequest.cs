using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    [Obsolete("Since v5.11")]
    public class DownloadQuestionnaireRequest
    {
        public Guid QuestionnaireId { get; set; }
        public QuestionnaireVersion SupportedVersion { get; set; }
    }
}
