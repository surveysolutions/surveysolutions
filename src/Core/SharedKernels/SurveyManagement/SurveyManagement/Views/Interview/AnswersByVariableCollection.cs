using System;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class MapReportPoint : IReadSideRepositoryEntity
    {
        protected MapReportPoint()
        {
        }

        public MapReportPoint(string id)
        {
            this.Id = id;
        }

        public virtual string Id { set; get; }

        public virtual Guid QuestionnaireId { get; set; }
        public virtual long QuestionnaireVersion { get; set; }
        public virtual string Variable { get; set; }

        public virtual Guid InterviewId { get; set; }
        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }
    }
}