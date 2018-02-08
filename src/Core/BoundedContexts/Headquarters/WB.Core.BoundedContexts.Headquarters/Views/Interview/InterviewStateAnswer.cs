using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewStateAnswer
    {
        public Guid Id { get; set; }
        public int[] RosterVector { get; set; }
        public int? AsInt { get; set; }
        public double? AsDouble { get; set; }
        public long? AsLong { get; set; }
        public string AsString { get; set; }
        public DateTime? AsDatetime { get; set; }
        public bool? AsBool { get; set; }
        public int[] AsIntArray { get; set; }
        public string AsList { get; set; }
        public string AsYesNo { get; set; }
        public string AsIntMatrix { get; set; }
        public string AsGps { get; set; }
        public string AsAudio { get; set; }
        public string AsArea { get; set; }
    }
}