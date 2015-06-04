using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class AllInterviewsFactory : IViewFactory<AllInterviewsInputModel, AllInterviewsView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> reader;

        public AllInterviewsFactory(IQueryableReadSideRepositoryReader<InterviewSummary> reader)
        {
            this.reader = reader;
        }

        public AllInterviewsView Load(AllInterviewsInputModel input)
        {

            var interviews = this.reader.Query(_ =>
            {
                var items = ApplyFilter(input, _);
                items = this.DefineOrderBy(items, input);

                return items.Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize)
                    .ToList();
            });


            var totalCount = this.reader.Query(_ => ApplyFilter(input, _).Count());
            var result = new AllInterviewsView
            {
                Page = input.Page,
                PageSize = input.PageSize,
                TotalCount = totalCount,
                Items = interviews.Select(x => new AllInterviewsViewItem
                {
                    FeaturedQuestions = x.AnswersToFeaturedQuestions.Select(a => new InterviewFeaturedQuestion()
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
                }).ToList()
            };
            return result;
        }

        private static IQueryable<InterviewSummary> ApplyFilter(AllInterviewsInputModel input, IQueryable<InterviewSummary> _)
        {
            var items = _.Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(input.SearchBy))
            {
                items = items.Where(x => x.AnswersToFeaturedQuestions.Any(a => a.Answer.Contains(input.SearchBy)));
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
            return items;
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
