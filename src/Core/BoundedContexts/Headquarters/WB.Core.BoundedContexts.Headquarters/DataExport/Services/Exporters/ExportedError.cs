using System;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    public class ExportedError : IReadSideRepositoryEntity
    {
        public virtual Guid InterviewId { get; set; }
        public virtual Guid EntityId { get; set; }
        public virtual int[] RosterVector { get; set; }
        public virtual EntityType EntityType { set; get; }
        public virtual int[] FailedValidationConditions { get; set; }
    }
}