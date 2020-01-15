using System;
using WB.Services.Export.Events.Assignment.Base;

namespace WB.Services.Export.Events.Assignment
{
    public class AssignmentAudioRecordingChanged : AssignmentEvent
    {
        public bool AudioRecording { get; set; }
    }
}
