using System;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class QuestionnaireListItem : IReadSideRepositoryEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string OwnerName { get; set; }
        public QuestionnaireVersion Version { get; set; }
    }
}
