using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.Views
{
    public class UnexpectedExceptionFromInterviewerView : IPlainStorageEntity<int?>
    {
        [PrimaryKey, AutoIncrement]
        public int? Id { get; set; }

        public Guid InterviewerId { get; set; }

        public string StackTrace { get; set; }
        
        public string Message { get; set; }
    }
}
