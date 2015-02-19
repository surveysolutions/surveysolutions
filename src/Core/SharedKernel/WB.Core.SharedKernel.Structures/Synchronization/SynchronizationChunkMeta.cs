using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SynchronizationChunkMeta
    {
        public SynchronizationChunkMeta(string id, long sortIndex, Guid? userId, string itemType)
        {
            Id = id;
            SortIndex = sortIndex;
            UserId = userId;
            ItemType = itemType;
        }

        public string Id { get; private set; }
        public long SortIndex { get; private set; }
        public Guid? UserId { get; private set; }
        public string ItemType { get; private set; }
    }

    public class PackagesToPullMeta
    {
        public PackagesToPullMeta(IEnumerable<SynchronizationChunkMeta> userPackages, IEnumerable<SynchronizationChunkMeta> questionnairePackages, IEnumerable<SynchronizationChunkMeta> interviewPackages)
        {
            UserPackages = userPackages;
            QuestionnairePackages = questionnairePackages;
            InterviewPackages = interviewPackages;
        }
        public IEnumerable<SynchronizationChunkMeta> UserPackages { get; private set; }
        public IEnumerable<SynchronizationChunkMeta> QuestionnairePackages { get; private set; }
        public IEnumerable<SynchronizationChunkMeta> InterviewPackages { get; private set; }
    }
}
