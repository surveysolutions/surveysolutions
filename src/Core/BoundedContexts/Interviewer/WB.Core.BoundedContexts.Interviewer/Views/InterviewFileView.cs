using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewFileView : IPlainStorageEntity
    {
        public int OID { get; set; }
        public string Id { get; set; }
        public byte[] File { get; set; }
    }
}