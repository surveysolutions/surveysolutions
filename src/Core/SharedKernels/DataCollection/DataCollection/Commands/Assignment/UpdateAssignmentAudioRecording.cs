using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentAudioRecording : AssignmentCommand
    {
        public bool AudioRecording { get; }

        public UpdateAssignmentAudioRecording(Guid publicKey, Guid userId, bool audioRecording, QuestionnaireIdentity questionnaireIdentity) 
            : base(publicKey, userId, questionnaireIdentity)
        {
            AudioRecording = audioRecording;
        }
    }
}
