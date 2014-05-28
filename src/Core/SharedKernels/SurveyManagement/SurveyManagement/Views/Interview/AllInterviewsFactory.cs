using System;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Utility;
using Main.Core.View;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class AllInterviewsFactory : IViewFactory<AllInterviewsInputModel, AllInterviewsView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviews;

        public AllInterviewsFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviews)
        {
            this.interviews = interviews;
        }

        public AllInterviewsView Load(AllInterviewsInputModel input)
        {
            Expression<Func<InterviewSummary, bool>> predicate = (s) => !s.IsDeleted;

            if (input.Status.HasValue)
            {
                predicate = predicate.AndCondition(x => (x.Status == input.Status));
            }

            if (input.TeamLeadId.HasValue)
            {
                predicate = predicate.AndCondition(x => (x.TeamLeadId == input.TeamLeadId.Value));
            }

            if (input.QuestionnaireId.HasValue)
            {
                predicate = predicate.AndCondition(x => (x.QuestionnaireId == input.QuestionnaireId));
            }

            if (input.QuestionnaireVersion.HasValue)
            {
                predicate = predicate.AndCondition(x => (x.QuestionnaireVersion == input.QuestionnaireVersion));
            }

            var interviewItems = this.DefineOrderBy(this.interviews.Query(_ => _.Where(predicate)), input)
                            .Skip((input.Page - 1) * input.PageSize)
                            .Take(input.PageSize).ToList();

            return new AllInterviewsView
                {
                    Page = input.Page,
                    PageSize = input.PageSize,
                    TotalCount = this.interviews.Query(_ => _.Count(predicate)),
                    Items = interviewItems.Select(x => new AllInterviewsViewItem()
                        {
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
                            ResponsibleRole = x.ResponsibleRole,
                            HasErrors = x.HasErrors,
                            Status = x.Status.ToString(),
                            CanDelete =    x.Status == InterviewStatus.Created
                                        || x.Status == InterviewStatus.SupervisorAssigned
                                        || x.Status == InterviewStatus.InterviewerAssigned
                                        || x.Status == InterviewStatus.SentToCapi,
                            QuestionnaireId = x.QuestionnaireId,
                            QuestionnaireVersion = x.QuestionnaireVersion,
                            CreatedOnClient = x.WasCreatedOnClient
                        })
                };
        }

        private IQueryable<InterviewSummary> DefineOrderBy(IQueryable<InterviewSummary> query,
                                                        AllInterviewsInputModel model)
        {
            var orderBy = model.Orders.FirstOrDefault();
            if (orderBy == null)
            {
                return query;
            }
            return query.OrderUsingSortExpression(model.Order).AsQueryable();
        }
    }
}
