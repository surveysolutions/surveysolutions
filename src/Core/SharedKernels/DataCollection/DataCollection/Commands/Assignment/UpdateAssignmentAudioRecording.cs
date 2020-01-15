using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentAudioRecording : AssignmentCommand
    {
        public bool AudioRecording { get; }

        public UpdateAssignmentAudioRecording(Guid publicKey, Guid userId, bool audioRecording) : base(publicKey, userId)
        {
            AudioRecording = audioRecording;
        }
    }
}
