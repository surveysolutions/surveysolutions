using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Fetching;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interviews
{
    public interface ITeamInterviewsFactory
    {
        TeamInterviewsView Load(TeamInterviewsInputModel input);
    }

    internal sealed class TeamInterviewsFactory : ITeamInterviewsFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> reader;

        public TeamInterviewsFactory(IQueryableReadSideRepositoryReader<InterviewSummary> reader)
        {
            this.reader = reader;
        }

        public TeamInterviewsView Load(TeamInterviewsInputModel input)
        {
            var interviewsPage = this.reader.Query(_ => 
            {
                var items = ApplyDynamicFilter(input, _);
                var seachIndexContents = this.DefineOrderBy(items, input)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .Select(x => x.SummaryId)
                    .ToArray();

                var summaries = DefineOrderBy(_, input)
                    .Where(x => seachIndexContents.Contains(x.SummaryId))
                    .Fetch(x => x.AnswersToFeaturedQuestions)
                    .ToList();

                return summaries
                    .Select(x => new TeamInterviewsViewItem {
                        FeaturedQuestions = 
                            x.AnswersToFeaturedQuestions.Select(a => new InterviewFeaturedQuestion
                            {
                                Id = a.Questionid,
                                Answer = a.Answer,
                                Question = a.Title,
                                Type = a.Type
                            }).ToList(),
                        InterviewId = x.InterviewId,
                        Key = x.Key,
                        ClientKey = x.ClientKey,
                        LastEntryDateUtc = x.UpdateDate,
                        ResponsibleId = x.ResponsibleId,
                        ResponsibleName = x.ResponsibleName,
                        ResponsibleRole = x.ResponsibleRole,
                        Status = x.Status.ToString(),
                        ErrorsCount = x.ErrorsCount,
                        CanBeReassigned = x.Status == InterviewStatus.Created
                                          || x.Status == InterviewStatus.SupervisorAssigned
                                          || x.Status == InterviewStatus.InterviewerAssigned
                                          || x.Status == InterviewStatus.RejectedBySupervisor,
                        CanApprove = x.Status == InterviewStatus.Completed || x.Status == InterviewStatus.RejectedByHeadquarters,
                        CanReject = x.Status == InterviewStatus.Completed || x.Status == InterviewStatus.RejectedByHeadquarters,
                        IsNeedInterviewerAssign = !x.IsAssignedToInterviewer,
                        AssignmentId = x.AssignmentId,
                        ReceivedByInterviewer = x.ReceivedByInterviewer
                    }).ToList();;
            });


            var totalCount = this.reader.Query(_ =>
            {
                var counter = ApplyDynamicFilter(input, _);
                return counter.Count();
            });

            
            return new TeamInterviewsView
            {
                TotalCount = totalCount,
                Items = interviewsPage
            };   
        }

        private static IQueryable<InterviewSummary> ApplyDynamicFilter(TeamInterviewsInputModel input, IQueryable<InterviewSummary> _)
        {
            var items = _
                .Where(x => x.Status != InterviewStatus.ApprovedBySupervisor && x.Status != InterviewStatus.ApprovedByHeadquarters);

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                items = items.Where(x => x.Key.StartsWith(input.SearchBy) || x.ClientKey.StartsWith(input.SearchBy) || x.AnswersToFeaturedQuestions.Any(a => a.Answer.ToLower().StartsWith(input.SearchBy.ToLower())));
            }

            if (input.Status.HasValue)
            {
                items = items.Where(x => (x.Status == input.Status));
            }

            if (input.QuestionnaireId.HasValue)
            {
                items = items.Where(x => (x.QuestionnaireId == input.QuestionnaireId));
            }

            if (input.QuestionnaireVersion.HasValue)
            {
                items = items.Where(x => (x.QuestionnaireVersion == input.QuestionnaireVersion));
            }

            if (input.ViewerId.HasValue)
            {
                items = items.Where(x => x.TeamLeadId == input.ViewerId);
            }
            if (!string.IsNullOrEmpty(input.ResponsibleName))
            {
                var lowerResponsibleName = input.ResponsibleName.ToLower();
                items = items.Where(x => x.ResponsibleName.ToLower() == lowerResponsibleName || x.TeamLeadName.ToLower() == lowerResponsibleName);
            }

            if (input.AssignmentId.HasValue)
            {
                items = items.Where(x => x.AssignmentId == input.AssignmentId);
            }

            return items;
        }

        private IQueryable<InterviewSummary> DefineOrderBy(IQueryable<InterviewSummary> query, TeamInterviewsInputModel model)
        {
            var orderBy = model.Orders?.FirstOrDefault();
            if (orderBy == null)
            {
                return query.OrderByDescending(x => x.UpdateDate);
            }
            return query.OrderUsingSortExpression(model.Order).AsQueryable();
        }

    }
}
