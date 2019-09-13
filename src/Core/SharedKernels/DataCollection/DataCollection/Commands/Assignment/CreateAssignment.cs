using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class CreateAssignment : AssignmentCommand
    {
        public int Id { get; }
        public QuestionnaireIdentity QuestionnaireId { get; }
        public Guid ResponsibleId { get; }
        public int? Quantity { get; }
        public bool IsAudioRecordingEnabled { get; }
        public string Email { get; }
        public string Password { get; }
        public bool? WebMode { get; }
        public List<InterviewAnswer> Answers { get; }
        public List<string> ProtectedVariables { get; }
        public string Comment { get; }

        public CreateAssignment(Guid assignmentId,
            int id,
            Guid userId,
            QuestionnaireIdentity questionnaireId,
            Guid responsibleId,
            int? quantity,
            bool isAudioRecordingEnabled,
            string email,
            string password,
            bool? webMode,
            List<InterviewAnswer> answers, 
            List<string> protectedVariables,
            string comment) 
            : base(assignmentId, userId)
        {
            Id = id;
            QuestionnaireId = questionnaireId;
            ResponsibleId = responsibleId;
            Quantity = quantity;
            IsAudioRecordingEnabled = isAudioRecordingEnabled;
            Email = email;
            Password = password;
            WebMode = webMode;
            Answers = answers ?? new List<InterviewAnswer>();
            ProtectedVariables = protectedVariables ?? new List<string>();
            Comment = comment;
        }
    }
}
