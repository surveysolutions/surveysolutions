using System;
using System.Collections.Generic;
using SQLite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class AssignmentDocument : IPlainStorageEntity<int>
    {
        [PrimaryKey]
        public virtual int Id { get; set; }

        public int? Quantity { get; set; }
        public string QuestionnaireId { get; set; }
        public string Title { get; set; }

        public Guid? LocationQuestionId { get; set; }

        public double? LocationLatitude { get; set; }
        public double? LocationLongitude { get; set; }

        public DateTime ReceivedDateUtc { get; set; }

        public Guid ResponsibleId { get; set; }

        public int? CreatedInterviewsCount { get; set; }

        [Ignore]
        public List<AssignmentAnswer> Answers { get; set; }

        [Ignore]
        public List<AssignmentAnswer> IdentifyingAnswers { get; set; } = new List<AssignmentAnswer>();

        [Ignore]
        public List<AssignmentProtectedVariable> ProtectedVariables { get; set; } = new List<AssignmentProtectedVariable>();

        public class AssignmentAnswer
        {
            [PrimaryKey, AutoIncrement]
            public long? Id { get; set; }

            [Indexed(Unique = false)]
            public int AssignmentId { get; set; }

            public bool IsIdentifying { get; set; }

            [Ignore]
            public Identity Identity { get; set; }

            public string IdentityValue
            {
                get => Identity?.ToString();
                set => Identity = Identity.Parse(value);
            }

            public string SerializedAnswer { get; set; }

            public string Question { get; set; }

            public string AnswerAsString { get; set; }
        }

        public class AssignmentProtectedVariable
        {
            [PrimaryKey, AutoIncrement]
            public long? Id { get; set; }

            public string Variable { get; set; }

            [Indexed(Unique = false)]
            public int AssignmentId { get; set; }
        }
    }
}
