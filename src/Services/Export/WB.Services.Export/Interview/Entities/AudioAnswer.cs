using System;
using System.Diagnostics;

namespace WB.Services.Export.Interview.Entities
{
    [DebuggerDisplay("{ToString()}")]
    public class AudioAnswer
    {
        protected AudioAnswer() { }

        private AudioAnswer(string fileName, TimeSpan length)
        {
            if (length.Ticks == 0) throw new ArgumentNullException(nameof(length));

            this.FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            this.Length = length;
        }

        public string FileName { get; }
        public TimeSpan Length { get; }

        public static AudioAnswer FromString(string fileName, TimeSpan? length)
        {
            return fileName != null ? new AudioAnswer(fileName, length.Value) : null;
        }

        public override string ToString() => $"{FileName} => {Length}";

        public override bool Equals(object obj)
        {
            var target = obj as AudioAnswer;
            if (target == null) return false;

            return target.Length == this.Length && target.FileName == this.FileName;
        }

        public override int GetHashCode() => this.Length.GetHashCode() ^ this.FileName.GetHashCode();
    }
}
