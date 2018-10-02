using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Fetching;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    internal sealed class AllInterviewsFactory : IAllInterviewsFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;

        public AllInterviewsFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public AllInterviewsView Load(AllInterviewsInputModel input)
        {
            var interviews = this.interviewSummaryReader.Query(_ =>
            {
                var items = ApplyFilter(input, _);
                items = this.DefineOrderBy(items, input);

                var ids = items.Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize)
                    .Select(x => x.SummaryId)
                    .ToList();

                var summaries = DefineOrderBy(_, input)
                    .Where(x => ids.Contains(x.SummaryId))
                    .Fetch(x => x.AnswersToFeaturedQuestions)
                    .ToList();
                return summaries;
            });

            var totalCount = this.interviewSummaryReader.Query(_ => ApplyFilter(input, _).Count());

            var result = new AllInterviewsView
            {
                Page = input.Page,
                PageSize = input.PageSize,
                TotalCount = totalCount,
                Items = interviews.Select(x => new AllInterviewsViewItem
                {
                    FeaturedQuestions = x.AnswersToFeaturedQuestions.Select(a => new InterviewFeaturedQuestion
                        {
                            Id = a.Questionid,
                            Answer = a.Answer,
                            Question = a.Title,
                            Type = a.Type
                        }).ToList(),
                    InterviewId = x.InterviewId,
                    LastEntryDateUtc = x.UpdateDate,
                    ResponsibleId = x.ResponsibleId,
                    ResponsibleName = x.ResponsibleName,
                    ResponsibleRole = x.ResponsibleRole,
                    ErrorsCount = x.ErrorsCount,
                    Status = x.Status.ToString(),
                    CanDelete = (x.Status == InterviewStatus.Created
                        || x.Status == InterviewStatus.SupervisorAssigned
                        || x.Status == InterviewStatus.InterviewerAssigned
                        || x.Status == InterviewStatus.SentToCapi) &&
                        !x.ReceivedByInterviewer && !x.WasCompleted,
                    CanApprove = x.Status == InterviewStatus.ApprovedBySupervisor || x.Status == InterviewStatus.Completed,
                    CanReject = x.Status == InterviewStatus.ApprovedBySupervisor || x.Status == InterviewStatus.Completed,
                    CanUnapprove = x.Status == InterviewStatus.ApprovedByHeadquarters,
                    CanBeReassigned = x.Status == InterviewStatus.SupervisorAssigned
                        || x.Status == InterviewStatus.InterviewerAssigned
                        || x.Status == InterviewStatus.Completed
                        || x.Status == InterviewStatus.RejectedBySupervisor
                        || x.Status == InterviewStatus.RejectedByHeadquarters,
                    QuestionnaireId = x.QuestionnaireId,
                    QuestionnaireVersion = x.QuestionnaireVersion,
                    AssignmentId = x.AssignmentId,
                    ReceivedByInterviewer = x.ReceivedByInterviewer,
                    TeamLeadName = x.TeamLeadName,
                    Key = x.Key,
                    ClientKey = x.ClientKey
                }).ToList()
            };
            return result;
        }

        public InterviewsWithoutPrefilledView LoadInterviewsWithoutPrefilled(InterviewsWithoutPrefilledInputModel input)
        {
            List<InterviewSummary> interviews = new List<InterviewSummary>();
            int totalCount;
            if (input.InterviewId.HasValue)
            {
                var interviewSummary = this.interviewSummaryReader.GetById(input.InterviewId.Value);
                if (interviewSummary!=null)
                {
                    interviews = interviewSummary.ToEnumerable().ToList();
                }
                totalCount = interviews.Count;
            }
            else
            {
                interviews = this.interviewSummaryReader.Query(_ =>
                {
                    var items = ApplyFilter(input, _);
                    if (input.Orders != null)
                    {
                        items = this.DefineOrderBy(items, input);
                    }

                    return items.Skip((input.Page - 1)*input.PageSize)
                        .Take(input.PageSize)
                        .ToList();
                });

                totalCount = this.interviewSummaryReader.Query(_ => ApplyFilter(input, _).Count());
            }
            var result = new InterviewsWithoutPrefilledView
            {
                TotalCount = totalCount,
                Items = interviews.Select(x => new InterviewListItem
                {
                    InterviewId = x.InterviewId,
                    Key = x.Key,
                    QuestionnaireId = new QuestionnaireIdentity(x.QuestionnaireId, x.QuestionnaireVersion).ToString(),
                    ResponsibleId = x.ResponsibleId,
                    ResponsibleName = x.ResponsibleName,
                    ResponsibleRole = x.ResponsibleRole,
                    TeamLeadId = x.TeamLeadId,
                    TeamLeadName = x.TeamLeadName,
                    Status = x.Status,
                    UpdateDate = x.UpdateDate.ToLocalTime().FormatDateWithTime(),
                    WasCreatedOnClient = x.WasCreatedOnClient,
                    ReceivedByInterviewer = x.ReceivedByInterviewer,
                }).ToList()
            };
            return result;
        }

        private static IQueryable<InterviewSummary> ApplyFilter(InterviewsWithoutPrefilledInputModel input, IQueryable<InterviewSummary> _)
        {
            var items = _;

            if (input.SupervisorId.HasValue)
            {
                items = items.Where(x => x.TeamLeadId == input.SupervisorId);
            }

            if (!string.IsNullOrWhiteSpace(input.InterviewKey))
            {
                items = items.Where(x => x.Key == input.InterviewKey);
            }

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                items = items.Where(x => x.Key.StartsWith(input.SearchBy));
            }

            if (input.CensusOnly)
            {
                items = items.Where(x => x.WasCreatedOnClient);
            }

            if (input.InterviewerId.HasValue)
            {
                items = items.Where(x => x.ResponsibleId == input.InterviewerId);
            }

            if (input.QuestionnaireId!=null)
            {
                items = items.Where(x => x.QuestionnaireId == input.QuestionnaireId.QuestionnaireId && x.QuestionnaireVersion == input.QuestionnaireId.Version);
            }

            if (input.ChangedFrom.HasValue)
            {
                items = items.Where(x => x.UpdateDate >= input.ChangedFrom.Value);
            }

            if (input.ChangedTo.HasValue)
            {
                items = items.Where(x => x.UpdateDate <= input.ChangedTo.Value);
            }

            return items;
        }

        private IQueryable<InterviewSummary> ApplyFilter(AllInterviewsInputModel input, IQueryable<InterviewSummary> _)
        {
            var items = _;

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                var searchLowerCase = input.SearchBy.ToLower();
                items = items.Where(x => x.Key.StartsWith(searchLowerCase) || 
                                         x.ClientKey.StartsWith(searchLowerCase) || 
                                         x.AnswersToFeaturedQuestions.Any(a => a.Answer.ToLower().StartsWith(searchLowerCase)));
            }
            
            if (input.Statuses != null)
            {
                items = items.Where(x => input.Statuses.Contains(x.Status));
            }

            if (!string.IsNullOrWhiteSpace(input.SupervisorOrInterviewerName))
            {
                items = items.Where(x => x.TeamLeadName == input.SupervisorOrInterviewerName || x.ResponsibleName == input.SupervisorOrInterviewerName);
            }

            if (input.ResponsibleId.HasValue)
            {
                items = items.Where(x => x.ResponsibleId == input.ResponsibleId.Value);
            }

            if (input.TeamId.HasValue)
            {
                items = items.Where(x => x.ResponsibleId == input.TeamId || x.TeamLeadId == input.TeamId);
            }

            if (input.QuestionnaireId.HasValue)
            {
                items = items.Where(x => x.QuestionnaireId == input.QuestionnaireId);
            }

            if (input.QuestionnaireVersion.HasValue)
            {
                items = items.Where(x => x.QuestionnaireVersion == input.QuestionnaireVersion);
            }

            if (input.AssignmentId.HasValue)
            {
                items = items.Where(x => x.AssignmentId == input.AssignmentId);
            }

            if (input.UnactiveDateStart.HasValue || input.UnactiveDateEnd.HasValue)
            {
                items = from i in items
                let statusChangeTime = i.InterviewCommentedStatuses
                    .Where(s => 
                        (s.Status == InterviewExportedAction.Completed && i.Status == InterviewStatus.Completed)
                        || (s.Status == InterviewExportedAction.ApprovedBySupervisor && i.Status == InterviewStatus.ApprovedBySupervisor)
                        || (s.Status == InterviewExportedAction.ApprovedByHeadquarter && i.Status == InterviewStatus.ApprovedByHeadquarters)
                        || (s.Status == InterviewExportedAction.RejectedBySupervisor && i.Status == InterviewStatus.RejectedBySupervisor)
                        || (s.Status == InterviewExportedAction.RejectedByHeadquarter && i.Status == InterviewStatus.RejectedByHeadquarters)
                    )
                    .OrderByDescending(s => s.Position)
                    .Select(s => s.Timestamp)
                    .First()
                where (input.UnactiveDateStart <= statusChangeTime || input.UnactiveDateStart == null)
                   && (statusChangeTime <= input.UnactiveDateEnd || input.UnactiveDateEnd == null)
                select i;
            }

            return items;
        }

        private IQueryable<InterviewSummary> DefineOrderBy(IQueryable<InterviewSummary> query, ListViewModelBase model)
        {
            var orderBy = model.Orders?.FirstOrDefault();
            if (orderBy == null)
            {
                return query.OrderByDescending(x=>x.UpdateDate);
            }
            return query.OrderUsingSortExpression(model.Order).AsQueryable();
        }
    }
}
