using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentCreated : AssignmentEvent
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

        public AssignmentCreated(Guid userId, 
            DateTimeOffset originDate,
            QuestionnaireIdentity questionnaireId,
            Guid responsibleId,
            int? quantity,
            bool isAudioRecordingEnabled,
            string email,
            string password,
            bool? webMode, 
            List<InterviewAnswer> answers, 
            List<string> protectedVariables) 
            : base(userId, originDate)
        {
            QuestionnaireId = questionnaireId;
            ResponsibleId = responsibleId;
            Quantity = quantity;
            IsAudioRecordingEnabled = isAudioRecordingEnabled;
            Email = email;
            Password = password;
            WebMode = webMode;
            Answers = answers;
            ProtectedVariables = protectedVariables;
        }
    }
}
