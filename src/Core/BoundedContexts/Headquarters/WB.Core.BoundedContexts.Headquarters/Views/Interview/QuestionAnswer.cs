using System;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class QuestionAnswer : IReadSideRepositoryEntity
    {
        public virtual int Id { get; set; }

        public virtual QuestionnaireCompositeItem Question { get; set; }
        public virtual string Answer { get; set; }
        public virtual decimal? AnswerCode { get; set; }
        public virtual InterviewSummary InterviewSummary { get; set; }
        public virtual int Position { get; set; }
        public virtual string Variable { get; set; }
        public virtual string Title { get; set; }

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
