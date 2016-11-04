using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interviews
{
    public interface ITeamInterviewsFactory
    {
        TeamInterviewsView Load(TeamInterviewsInputModel input);
    }

    public class TeamInterviewsFactory : ITeamInterviewsFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> reader;
        private readonly IQueryableReadSideRepositoryReader<QuestionAnswer> featuredQuestionAnswersReader;

        public TeamInterviewsFactory(IQueryableReadSideRepositoryReader<InterviewSummary> reader,
            IQueryableReadSideRepositoryReader<QuestionAnswer> featuredQuestionAnswersReader)
        {
            this.reader = reader;
            this.featuredQuestionAnswersReader = featuredQuestionAnswersReader;
        }

        public TeamInterviewsView Load(TeamInterviewsInputModel input)
        {
            var interviewsPage = this.reader.Query(_ => 
            {
                var items = ApplyDynamicFilter(input, _);
                var seachIndexContents = this.DefineOrderBy(items, input)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList();
                return seachIndexContents;
            });


            var totalCount = this.reader.Query(_ =>
            {
                var counter = ApplyDynamicFilter(input, _);
                return counter.Count();
            });

            var selectedSummaries = interviewsPage.Select(x => x.SummaryId).ToArray();
            var answersToFeaturedQuestions = this.featuredQuestionAnswersReader.Query(_ => _.Where(x => selectedSummaries.Contains(x.InterviewSummary.SummaryId)).ToList());

            var teamInterviewsViewItems = interviewsPage
                .Select(x => new TeamInterviewsViewItem {
                    FeaturedQuestions = 
                        answersToFeaturedQuestions.Where(a => a.InterviewSummary.SummaryId == x.SummaryId).Select(a => new InterviewFeaturedQuestion
                        {
                            Id = a.Questionid,
                            Answer = a.Answer,
                            Question = a.Title,
                            Type = a.Type
                        }).ToList(),
                    InterviewId = x.InterviewId,
                    LastEntryDate = x.UpdateDate.ToShortDateString(),
                    ResponsibleId = x.ResponsibleId,
                    ResponsibleName = x.ResponsibleName,
                    Status = x.Status.ToString(),
                    HasErrors = x.HasErrors,
                    CanBeReassigned = x.Status == InterviewStatus.Created
                        || x.Status == InterviewStatus.SupervisorAssigned
                        || x.Status == InterviewStatus.InterviewerAssigned
                        || x.Status == InterviewStatus.RejectedBySupervisor,
                    CanApprove = x.Status == InterviewStatus.Completed || x.Status == InterviewStatus.RejectedByHeadquarters,
                    CanReject = x.Status == InterviewStatus.Completed || x.Status == InterviewStatus.RejectedByHeadquarters,
                    CreatedOnClient = x.WasCreatedOnClient,
                    ReceivedByInterviewer = x.ReceivedByInterviewer
                }).ToList();
            return new TeamInterviewsView
            {
                TotalCount = totalCount,
                Items = teamInterviewsViewItems
            };   
        }

        private static IQueryable<InterviewSummary> ApplyDynamicFilter(TeamInterviewsInputModel input, IQueryable<InterviewSummary> _)
        {
            var items = _
                .Where(x => !x.IsDeleted)
                .Where(x => (x.Status != InterviewStatus.ApprovedBySupervisor && x.Status != InterviewStatus.ApprovedByHeadquarters));

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                items = items.Where(x => x.AnswersToFeaturedQuestions.Any(a => a.Answer.ToLower().Contains(input.SearchBy.ToLower())));
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

            return items;
        }

        private IQueryable<InterviewSummary> DefineOrderBy(IQueryable<InterviewSummary> query, TeamInterviewsInputModel model)
        {
            var orderBy = model.Orders.FirstOrDefault();
            if (orderBy == null)
            {
                return query.OrderByDescending(x => x.UpdateDate);
            }
            return query.OrderUsingSortExpression(model.Order).AsQueryable();
        }

    }
}
