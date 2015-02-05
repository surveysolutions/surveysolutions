using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SyncItemsMetaContainer
    {
        public IEnumerable<SynchronizationChunkMeta> UserChunksMeta { get; set; }

        public IEnumerable<SynchronizationChunkMeta> QuestionnaireChunksMeta { get; set; }

        public IEnumerable<SynchronizationChunkMeta> InterviewChunksMeta { get; set; }
    }

}