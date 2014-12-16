using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class DownloadQuestionnaireRequest
    {
        public Guid QuestionnaireId { get; set; }
        public int SupportedVersionMajor { get; set; }
        public int SupportedVersionMinor { get; set; }
        public int SupportedVersionPatch { get; set; }
    }
}
