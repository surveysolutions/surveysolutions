using System;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class IdentifyEntityValue : IReadSideRepositoryEntity
    {
        private string value;
        public virtual int Id { get; set; }

        public virtual QuestionnaireCompositeItem Entity { get; set; }

        public virtual string Value
        {
            get => value;
            set
            {
                this.value = value;
                this.ValueLowerCase = value?.ToLower();
            }
        }

        public virtual string ValueLowerCase { get; protected set; }
        
        public virtual decimal? AnswerCode { get; set; }
        public virtual InterviewSummary InterviewSummary { get; set; }
        public virtual int Position { get; set; }
        
        protected bool Equals(IdentifyEntityValue other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IdentifyEntityValue) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
