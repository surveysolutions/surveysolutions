using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentAudioRecordingChanged : AssignmentEvent
    {
        public bool AudioRecording { get; }

        public AssignmentAudioRecordingChanged(Guid userId, DateTimeOffset originDate, bool audioRecording) 
            : base(userId, originDate)
        {
            this.AudioRecording = audioRecording;
        }
    }
}
