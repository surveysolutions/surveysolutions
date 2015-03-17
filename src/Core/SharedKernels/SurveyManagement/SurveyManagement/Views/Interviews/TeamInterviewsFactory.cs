using System.Linq;
using NHibernate.Linq;
using Raven.Client;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviews
{
    public class TeamInterviewsFactory : IViewFactory<TeamInterviewsInputModel, TeamInterviewsView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> reader;

        public TeamInterviewsFactory(IQueryableReadSideRepositoryReader<InterviewSummary> reader)
        {
            this.reader = reader;
        }

        public TeamInterviewsView Load(TeamInterviewsInputModel input)
        {
            var interviewsPage = reader.Query(_ => 
            {
                var items = ApplyDynamicFilter(input, _);
                var seachIndexContents = this.DefineOrderBy(items, input)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList();
                return seachIndexContents;
            });


            var totalCount = reader.Query(_ =>
            {
                var counter = ApplyDynamicFilter(input, _);
                return counter.Count();
            }); 

            var teamInterviewsViewItems = interviewsPage
                .Select(x => new TeamInterviewsViewItem {
                    FeaturedQuestions = x.AnswersToFeaturedQuestions.Select(a => new InterviewFeaturedQuestion
                    {
                        Id = a.Id,
                        Answer = a.Answer,
                        Question = a.Title
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
                    CanApproveOrReject = x.Status == InterviewStatus.Completed
                        || x.Status == InterviewStatus.RejectedByHeadquarters,
                    CreatedOnClient = x.WasCreatedOnClient
                }).ToList();
            return new TeamInterviewsView
            {
                TotalCount = totalCount,
                Items = teamInterviewsViewItems
            };   
        }

        private static IQueryable<InterviewSummary> ApplyDynamicFilter(TeamInterviewsInputModel input, IQueryable<InterviewSummary> _)
        {
            var items = _;
            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                items = items.Where(x => x.AnswersToFeaturedQuestions.Any(a => a.Answer.Contains(input.SearchBy)));
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

            items = input.ResponsibleId.HasValue
                ? items.Where(x => x.ResponsibleId == input.ResponsibleId)
                : items.Where(x => x.TeamLeadId == input.ViewerId);
            return items;
        }

        private IQueryable<InterviewSummary> DefineOrderBy(IQueryable<InterviewSummary> query, TeamInterviewsInputModel model)
        {
            var orderBy = model.Orders.FirstOrDefault();
            if (orderBy == null)
            {
                return query.OrderByDescending(x=>x.UpdateDate);
            }
            return query.OrderUsingSortExpression(model.Order).AsQueryable();
        }

    }
}
