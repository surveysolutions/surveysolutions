using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class QuestionnaireMetaInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime LastEntryDate { get; set; }
        public string OwnerName { get; set; }
        public QuestionnaireVersion Version { get; set; }
        public string Email { get; set; }
    }
}
