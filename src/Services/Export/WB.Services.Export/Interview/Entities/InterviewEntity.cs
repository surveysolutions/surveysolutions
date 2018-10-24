using System;
using System.Diagnostics;

namespace WB.Services.Export.Interview.Entities
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewStringAnswer
    {
        public Guid InterviewId { get; set; }
        public string Answer { get; set; }

        public override bool Equals(object obj)
        {
            var target = obj as InterviewStringAnswer;
            if (target == null) return false;

            return this.Equals(target);
        }

        protected bool Equals(InterviewStringAnswer other)
        {
            return InterviewId.Equals(other.InterviewId) && string.Equals(Answer, other.Answer);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (InterviewId.GetHashCode() * 397) ^ (Answer != null ? Answer.GetHashCode() : 0);
            }
        }

        public override string ToString() => $"{InterviewId} => {Answer}";
    }

    [DebuggerDisplay("InterviewId = {InterviewId}; Identity = {Identity}; EntityType = {EntityType}")]
    public class InterviewEntity
    {
        public virtual Guid InterviewId { get; set; }
        public virtual Identity Identity { get; set; }
        public virtual EntityType EntityType { get; set; }

        public virtual bool HasFlag { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual int[] InvalidValidations { get; set; }
        public virtual int[] WarningValidations { get; set; }
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

        public object AsObject()
            => this.AsString ?? this.AsInt ?? this.AsDouble ??
               this.AsDateTime ?? this.AsLong ??
               this.AsBool ?? this.AsGps ?? this.AsIntArray ??
               this.AsList ?? this.AsYesNo ??
               this.AsIntMatrix ?? this.AsArea ??
               (object) this.AsAudio;

    }
}
