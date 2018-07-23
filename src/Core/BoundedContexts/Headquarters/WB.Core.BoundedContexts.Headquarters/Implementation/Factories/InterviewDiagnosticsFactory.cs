using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    public class InterviewDiagnosticsFactory : IInterviewDiagnosticsFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;

        public InterviewDiagnosticsFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public InterviewDiagnosticsInfo GetById(Guid interviewId)
        {
            return GetByBatchIds(interviewId.ToEnumerable()).SingleOrDefault();
        }

        public List<InterviewDiagnosticsInfo> GetByBatchIds(IEnumerable<Guid> interviewIds)
        {
            return interviewSummaryReader.Query(_ =>
            {
                var interviews = _.Where(i => interviewIds.Contains(i.InterviewId));

                return interviews.Select(i => new InterviewDiagnosticsInfo()
                {
                    InterviewId = i.InterviewId,
                    InterviewKey = i.Key,
                    Status = i.Status,
                    ResponsibleId = i.ResponsibleId,
                    ResponsibleName = i.ResponsibleName,
                    NumberOfInterviewers = i.InterviewCommentedStatuses.Where(s => s.Status == InterviewExportedAction.InterviewerAssigned).Select(s => s.InterviewerId).Where(s => s != null).Distinct().Count(),
                    NumberRejectionsBySupervisor = i.InterviewCommentedStatuses.Where(s => s.Status == InterviewExportedAction.RejectedBySupervisor).Count(),
                    NumberRejectionsByHq = i.InterviewCommentedStatuses.Where(s => s.Status == InterviewExportedAction.RejectedByHeadquarter).Count(),
                    NumberValidQuestions = 0,
                    NumberInvalidEntities = i.ErrorsCount,
                    NumberUnansweredQuestions = 0,
                    NumberCommentedQuestions = i.CommentedEntitiesCount,
                    InterviewDuration = i.InterviewDurationLong,
                }).OrderBy(x => x.InterviewId);
            }).ToList();
        }
    }
}
