using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public abstract class AbstractInterviewDbEntity : IView
    {
        public virtual int Id { get; set; }
        public virtual Guid InterviewId { get; set; }
        public virtual Identity Identity { get; set; }
        public virtual EntityType EntityType { get; set; }

    }
}