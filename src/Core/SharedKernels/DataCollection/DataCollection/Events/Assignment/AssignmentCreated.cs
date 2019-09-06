using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentCreated : AssignmentEvent
    {
        public QuestionnaireIdentity QuestionnaireId { get; }
        public Guid ResponsibleId { get; }
        public int? Quantity { get; }
        public bool IsAudioRecordingEnabled { get; }
        public string Email { get; }
        public string Password { get; }
        public bool? WebMode { get; }

        public AssignmentCreated(Guid userId, 
            DateTimeOffset originDate,
            QuestionnaireIdentity questionnaireId,
            Guid responsibleId,
            int? quantity,
            bool isAudioRecordingEnabled,
            string email,
            string password,
            bool? webMode) 
            : base(userId, originDate)
        {
            QuestionnaireId = questionnaireId;
            ResponsibleId = responsibleId;
            Quantity = quantity;
            IsAudioRecordingEnabled = isAudioRecordingEnabled;
            Email = email;
            Password = password;
            WebMode = webMode;
        }
    }
}
