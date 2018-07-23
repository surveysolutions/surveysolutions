using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class InterviewSequenceView : IPlainStorageEntity<Guid>
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public int ReceivedFromServerSequence { get; set; }
    }
}
