using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class QuestionAnswer : IReadSideRepositoryEntity
    {
        public virtual int Id { get; set; }

        public virtual Guid Questionid { get; set; }
        public virtual string Title { get; set; }
        public virtual string Answer { get; set; }
        public virtual QuestionType Type { get; set; }
        public virtual InterviewSummary InterviewSummary { get; set; }
        public virtual int Position { get; set; }
    }
}