using System.Linq;
using Raven.Client;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviews
{
    public class TeamInterviewsFactory : IViewFactory<TeamInterviewsInputModel, TeamInterviewsView>
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public TeamInterviewsFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public TeamInterviewsView Load(TeamInterviewsInputModel input)
        {
             string indexName = typeof(InterviewsSearchIndex).Name;

            var items = indexAccessor.Query<SeachIndexContent>(indexName);

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                items = items.Search(x => x.FeaturedQuestionsWithAnswers, input.SearchBy, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards, options: SearchOptions.And);
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

            var totalCount = items.Count();
            var seachIndexContents = this.DefineOrderBy(items.ProjectFromIndexFieldsInto<InterviewSummary>(), input)
                                         .Skip((input.Page - 1) * input.PageSize)
                                         .Take(input.PageSize)
                                         .ToList();

            var teamInterviewsViewItems = seachIndexContents
                .Select(x => new TeamInterviewsViewItem {
                    FeaturedQuestions = x.AnswersToFeaturedQuestions.Values.Select(a => new InterviewFeaturedQuestion()
                    {
                        Id = a.Id,
                        Answer = a.Answer,
                        Question = a.Title
                    }),
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
                });
            return new TeamInterviewsView
            {
                TotalCount = totalCount,
                Items = teamInterviewsViewItems
            };   
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
