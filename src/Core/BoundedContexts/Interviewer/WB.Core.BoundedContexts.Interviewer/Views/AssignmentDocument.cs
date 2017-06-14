using System;
using System.Collections.Generic;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class AssignmentDocument : IPlainStorageEntity<int>
    {
        [PrimaryKey]
        public virtual int Id { get; set; }

        public int? Quantity { get; set; }

        public int InterviewsCount { get; set; }

        public string QuestionnaireId { get; set; }
        public string Title { get; set; }

        public Guid? LocationQuestionId { get; set; }

        public double? LocationLatitude { get; set; }
        public double? LocationLongitude { get; set; }

        public DateTime ReceivedDateUtc { get; set; }

        [Ignore]
        public List<IdentifyingAnswer> IdentifyingData { get; set; } = new List<IdentifyingAnswer>();

        public class IdentifyingAnswer
        {
            [PrimaryKey, AutoIncrement]
            public long? Id { get; set; }

            [Indexed(Unique = false)]
            public int AssignmentId { get; set; }

            [Ignore]
            public Identity Identity { get; set; }

            public string IdentityValue
            {
                get => Identity?.ToString();
                set => Identity = Identity.Parse(value);
            }

            public string Answer { get; set; }
            public string Question { get; set; }
            public string AnswerAsString { get; set; }
        }
    }
}