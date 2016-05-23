using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    internal class HeadquartersTeamsAndStatusesReport : AbstractTeamsAndStatusesReport, IHeadquartersTeamsAndStatusesReport
    {
        public HeadquartersTeamsAndStatusesReport(INativeReadSideStorage<InterviewSummary> interviewsReader) : base(interviewsReader)
        {
        }

        protected override IQueryOver<InterviewSummary, InterviewSummary> ApplyFilter(TeamsAndStatusesInputModel input,
            IQueryOver<InterviewSummary, InterviewSummary> interviews)
        {
            var filteredInterviews = interviews.Where(x => !x.IsDeleted);

            if (input.TemplateId.HasValue)
            {
                filteredInterviews = filteredInterviews.Where(x => x.QuestionnaireId == input.TemplateId);
            }

            if (input.TemplateVersion.HasValue)
            {
                filteredInterviews = filteredInterviews.Where(x => x.QuestionnaireVersion == input.TemplateVersion);
            }

            if (input.ViewerId.HasValue)
            {
                filteredInterviews = filteredInterviews.Where(x => x.TeamLeadId == input.ViewerId);
            }

            return filteredInterviews;
        }

        protected override Expression<Func<InterviewSummary, object>> ResponsibleIdSelector
        {
            get { return (interivew) => interivew.TeamLeadId; }
        }

        protected override Expression<Func<InterviewSummary, object>> ResponsibleNameSelector
        {
            get { return (interivew) => interivew.TeamLeadName; }
        }
    }
}