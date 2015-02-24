using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class PackagesToPullMeta
    {
        public PackagesToPullMeta(IEnumerable<SynchronizationChunkMeta> userPackages, IEnumerable<SynchronizationChunkMeta> questionnairePackages, IEnumerable<SynchronizationChunkMeta> interviewPackages)
        {
            this.UserPackages = userPackages;
            this.QuestionnairePackages = questionnairePackages;
            this.InterviewPackages = interviewPackages;
        }
        public IEnumerable<SynchronizationChunkMeta> UserPackages { get; private set; }
        public IEnumerable<SynchronizationChunkMeta> QuestionnairePackages { get; private set; }
        public IEnumerable<SynchronizationChunkMeta> InterviewPackages { get; private set; }
    }
}