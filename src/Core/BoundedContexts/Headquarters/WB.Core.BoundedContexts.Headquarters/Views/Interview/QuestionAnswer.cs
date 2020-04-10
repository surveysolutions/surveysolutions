using System;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class QuestionAnswer : IReadSideRepositoryEntity
    {
        private string answer;
        public virtual int Id { get; set; }

        public virtual QuestionnaireCompositeItem Question { get; set; }

        public virtual string Answer
        {
            get => answer;
            set
            {
                answer = value;
                this.AnswerLowerCase = value?.ToLower();
            }
        }

        public virtual string AnswerLowerCase { get; protected set; }
        
        public virtual decimal? AnswerCode { get; set; }
        public virtual InterviewSummary InterviewSummary { get; set; }
        public virtual int Position { get; set; }
        
        protected bool Equals(QuestionAnswer other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QuestionAnswer) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
