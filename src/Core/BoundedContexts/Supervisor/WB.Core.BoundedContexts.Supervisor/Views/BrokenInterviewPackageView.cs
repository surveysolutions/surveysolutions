using System;
using SQLite;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.Views
{
    public class BrokenInterviewPackageView : IPlainStorageEntity<int?>
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int? Id { get; set; }
        public Guid InterviewId { get; set; }
        public string InterviewKey { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public Guid ResponsibleId { get; set; }
        public InterviewStatus InterviewStatus { get; set; }
        public string Events { get; set; }
        public DateTime IncomingDate { get; set; }
        public DateTime ProcessingDate { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionStackTrace { get; set; }
        public long PackageSize { get; set; }
    }
}
