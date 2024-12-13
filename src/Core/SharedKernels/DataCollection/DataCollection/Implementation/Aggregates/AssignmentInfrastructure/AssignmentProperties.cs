using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.AssignmentInfrastructure
{
    public class AssignmentProperties
    {
        public int Id { get; set; }

        public Guid PublicKey { get; set; }

        public Guid ResponsibleId { get; set; }

        public int? Quantity { get; set; }

        public bool Archived { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public DateTimeOffset? ReceivedByTabletAt { get; set; }

        public QuestionnaireIdentity QuestionnaireId { get; set; }

        public bool AudioRecording { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public bool? WebMode { get; set; }

        public IList<InterviewAnswer> Answers { get; set; }

        public IList<string> ProtectedVariables { get; set; }

        public bool IsDeleted { get; set; }
        public string Comment { get; set; }
        
        public string TargetArea { get; set; }
    }
}
