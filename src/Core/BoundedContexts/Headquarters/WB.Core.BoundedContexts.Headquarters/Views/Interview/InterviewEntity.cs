using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewEntity : IView
    {
        public virtual Guid InterviewId { get; set; }
        public virtual Identity Identity { get; set; }
        public virtual EntityType EntityType { get; set; }

        public virtual bool HasFlag { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual int[] InvalidValidations { get; set; }
        public virtual bool IsReadonly { get; set; }
        public virtual int? AsInt { get; set; }
        public virtual double? AsDouble { get; set; }
        public virtual long? AsLong { get; set; }
        public virtual string AsString { get; set; }
        public virtual DateTime? AsDateTime { get; set; }
        public virtual bool? AsBool { get; set; }
        public virtual int[] AsIntArray { get; set; }
        public virtual InterviewTextListAnswer[] AsList { get; set; }
        public virtual AnsweredYesNoOption[] AsYesNo { get; set; }
        public virtual int[][] AsIntMatrix { get; set; }
        public virtual GeoPosition AsGps { get; set; }
        public virtual AudioAnswer AsAudio { get; set; }
        public virtual Area AsArea { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is InterviewEntity target)) return false;

            return this.InterviewId == target.InterviewId && this.Identity == target.Identity;
        }

        public override int GetHashCode() => this.InterviewId.GetHashCode() ^ this.Identity.GetHashCode();

        public bool IsAllFieldsDefault() 
        {
            return !IsEnabled
                   && !IsReadonly
                   && !HasFlag
                   && (InvalidValidations == null || InvalidValidations.Length == 0)
                   && AsArea == null && AsAudio == null && AsBool == null && AsDateTime == null && AsDouble == null
                   && AsGps == null && AsInt == null && (AsIntArray == null || AsIntArray.Length == 0)
                   && AsIntMatrix == null && AsList == null && AsString == null && AsYesNo == null;
        }
    }
}