using System;
using System.Linq;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre;

#nullable enable
namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map
{
    public class MapReportResolver
    {
       public IQueryable<GpsAnswerQuery> GetMapReport(
            Guid questionnaireId,
            long? questionnaireVersion,
            string variable,
            int zoom,
            int clientMapWidth,
            double east,
            double west,
            double north,
            double south,
            [Service] IUnitOfWork unitOfWork,
            [Service]IAuthorizedUser authorizedUser)
        {
            var gps= unitOfWork.Session
                    .Query<InterviewGps>()
                    .Join(unitOfWork.Session.Query<InterviewSummary>(),
                        gps => gps.InterviewSummary.Id,
                        interview => interview.Id,
                        (gps, interview) => new  { gps, interview})
                    .Join(unitOfWork.Session.Query<QuestionnaireCompositeItem>(),
                        interview_gps => new { interview_gps.interview.QuestionnaireIdentity, interview_gps.gps.QuestionId },
                        questionnaireItem => new { questionnaireItem.QuestionnaireIdentity, QuestionId = questionnaireItem.EntityId },
                        (interview_gps, questionnaireItem) 
                            => new GpsAnswerQuery
                        {
                            InterviewSummary = interview_gps.interview,
                            Answer = interview_gps.gps,
                            QuestionnaireItem = questionnaireItem
                        });
            
            var gpsQuery = gps
                .Where(x => x.Answer.IsEnabled &&
                            x.QuestionnaireItem.StataExportCaption == variable &&
                            x.InterviewSummary.QuestionnaireId == questionnaireId);

            if (questionnaireVersion.HasValue)
            {
                gpsQuery = gpsQuery
                    .Where(x => x.InterviewSummary.QuestionnaireVersion == questionnaireVersion.Value);
            }

            var supervisorId = authorizedUser.IsSupervisor ? authorizedUser.Id : (Guid?) null;

            if (supervisorId.HasValue)
            {
                gpsQuery = gpsQuery
                    .Where(x => x.InterviewSummary.SupervisorId == supervisorId.Value 
                                && !DisabledStatusesForGps.Contains(x.InterviewSummary.Status));
            }

            return gpsQuery;
        }

        private static readonly InterviewStatus[] DisabledStatusesForGps =
        {
            InterviewStatus.ApprovedBySupervisor,
            InterviewStatus.ApprovedByHeadquarters
        };
    }
}
