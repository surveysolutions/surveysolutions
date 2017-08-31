using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class DbQuestion : IView
    {
        public virtual Guid InterviewId { get; set; }
        public virtual Identity QuestionIdentity { get; set; }

        public virtual bool IsFlagged { get; set; }
    }
}