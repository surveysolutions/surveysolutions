using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public class InterviewCommentaries: IView
    {
        public InterviewCommentaries()
        {
            this.Commentaries = new List<InterviewComment>();
        }
        public virtual bool IsDeleted { get; set; }
        public virtual bool IsApprovedByHQ { get; set; }
        public virtual string InterviewId { get; set; }
        public virtual string QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual IList<InterviewComment> Commentaries { get; set; }
    }
}