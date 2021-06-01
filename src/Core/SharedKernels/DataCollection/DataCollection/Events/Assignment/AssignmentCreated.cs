using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentCreated : AssignmentEvent
    {
        public int Id { get; }
        public Guid QuestionnaireId { get; }
        public long QuestionnaireVersion { get; }
        public Guid ResponsibleId { get; }
        public int? Quantity { get; }
        public bool AudioRecording { get; }
        public string Email { get; }
        public string Password { get; }
        public bool? WebMode { get; }
        public InterviewAnswer[] Answers { get; }
        public string[] ProtectedVariables { get; }
        public string Comment { get; }
        public int? UpgradedFromId { get; }

        public AssignmentCreated(Guid userId, 
            int id,
            DateTimeOffset originDate,
            Guid questionnaireId,
            long questionnaireVersion,
            Guid responsibleId,
            int? quantity,
            bool audioRecording,
            string email,
            string password,
            bool? webMode, 
            InterviewAnswer[] answers, 
            string[] protectedVariables,
            string comment,
            int? upgradedFromId) 
            : base(userId, originDate)
        {
            Id = id;
            QuestionnaireId = questionnaireId;
            QuestionnaireVersion = questionnaireVersion;
            ResponsibleId = responsibleId;
            Quantity = quantity;
            AudioRecording = audioRecording;
            Email = email;
            Password = password;
            WebMode = webMode;
            Answers = answers;
            ProtectedVariables = protectedVariables;
            Comment = comment;
            UpgradedFromId = upgradedFromId;
        }
    }
}
