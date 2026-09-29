using System;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    // Describes what the tablet should currently be recording. Distinguishes "nothing",
    // "whole interview" (audio audit flag) and a specific scoped group without overloading any
    // group id value, so a group whose id happens to be Guid.Empty cannot be confused with
    // whole-interview recording.
    public readonly struct RecordingTarget : IEquatable<RecordingTarget>
    {
        public static readonly RecordingTarget None = new RecordingTarget(false, null);
        public static readonly RecordingTarget WholeInterview = new RecordingTarget(true, null);
        public static RecordingTarget Group(Guid groupId) => new RecordingTarget(true, groupId);

        private RecordingTarget(bool isRecording, Guid? groupId)
        {
            this.IsRecording = isRecording;
            this.GroupId = groupId;
        }

        public bool IsRecording { get; }
        public Guid? GroupId { get; }

        public bool Equals(RecordingTarget other) =>
            this.IsRecording == other.IsRecording && this.GroupId == other.GroupId;

        public override bool Equals(object? obj) => obj is RecordingTarget other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.IsRecording, this.GroupId);
    }
}
