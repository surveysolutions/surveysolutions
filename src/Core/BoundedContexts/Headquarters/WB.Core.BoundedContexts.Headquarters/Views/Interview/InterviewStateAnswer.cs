using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;

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
        public InterviewTextListAnswer[] AsList { get; set; }
        public AnsweredYesNoOption[] AsYesNo { get; set; }
        public int[][] AsIntMatrix { get; set; }
        public GeoPosition AsGps { get; set; }
        public AudioAnswer AsAudio { get; set; }
        public Area AsArea { get; set; }
    }
}
