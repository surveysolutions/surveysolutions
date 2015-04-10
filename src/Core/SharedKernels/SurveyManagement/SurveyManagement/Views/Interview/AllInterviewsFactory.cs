using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class AllInterviewsFactory : IViewFactory<AllInterviewsInputModel, AllInterviewsView>
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public AllInterviewsFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public AllInterviewsView Load(AllInterviewsInputModel input)
        {
            string indexName = typeof(InterviewsSearchIndex).Name;

            var items = indexAccessor.Query<SeachIndexContent>(indexName);
            
            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                items = items.Search(x => x.FeaturedQuestionsWithAnswers, input.SearchBy, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards, options: SearchOptions.And);
            }

            if (input.Status.HasValue)
            {
                items = items.Where(x => x.Status == input.Status);
            }

            if (input.TeamLeadId.HasValue)
            {
                items = items.Where(x => x.TeamLeadId == input.TeamLeadId.Value);
            }

            if (input.QuestionnaireId.HasValue)
            {
                items = items.Where(x => x.QuestionnaireId == input.QuestionnaireId);
            }

            if (input.QuestionnaireVersion.HasValue)
            {
                items = items.Where(x => x.QuestionnaireVersion == input.QuestionnaireVersion);
            }

            List<InterviewSummary> interviewItems = this.DefineOrderBy(items.ProjectFromIndexFieldsInto<InterviewSummary>(), input)
                            .Skip((input.Page - 1) * input.PageSize)
                            .Take(input.PageSize)
                            .ToList();

            var result = new AllInterviewsView
            {
                Page = input.Page,
                PageSize = input.PageSize,
                TotalCount = items.Count(),
                Items = interviewItems.Select(x => new AllInterviewsViewItem
                {
                    FeaturedQuestions = x.AnswersToFeaturedQuestions.Values.Select(a => new InterviewFeaturedQuestion()
                    {
                        Id = a.Id,
                        Answer = a.Answer,
                        Question = a.Title,
                        Type = a.Type
                    }),
                    InterviewId = x.InterviewId,
                    LastEntryDate = x.UpdateDate.ToShortDateString(),
                    ResponsibleId = x.ResponsibleId,
                    ResponsibleName = x.ResponsibleName,
                    ResponsibleRole = x.ResponsibleRole,
                    HasErrors = x.HasErrors,
                    Status = x.Status.ToString(),
                    CanDelete =    x.Status == InterviewStatus.Created
                        || x.Status == InterviewStatus.SupervisorAssigned
                        || x.Status == InterviewStatus.InterviewerAssigned
                        || x.Status == InterviewStatus.SentToCapi,
                    CanApproveOrReject = x.Status == InterviewStatus.ApprovedBySupervisor,
                    QuestionnaireId = x.QuestionnaireId,
                    QuestionnaireVersion = x.QuestionnaireVersion,
                    CreatedOnClient = x.WasCreatedOnClient
                })
            };
            return result;
        }

        private IQueryable<InterviewSummary> DefineOrderBy(IQueryable<InterviewSummary> query,
                                                        AllInterviewsInputModel model)
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
