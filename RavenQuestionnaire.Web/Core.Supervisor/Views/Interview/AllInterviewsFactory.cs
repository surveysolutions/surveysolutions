using System;
using System.Linq.Expressions;
using Core.Supervisor.Views.Survey;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Raven.Client.Linq;
using Raven.Database.Linq.PrivateExtensions;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace Core.Supervisor.Views.Interview
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.View;

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

            if (input.QuestionnaireId.HasValue)
            {
                predicate = predicate.AndCondition(x => (x.QuestionnaireId == input.QuestionnaireId));
            }

            var interviewItems = DefineOrderBy(this.interviews.Query(_ => _.Where(predicate)), input)
                            .Skip((input.Page - 1) * input.PageSize)
                            .Take(input.PageSize).ToList();

            return new AllInterviewsView
                {
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
                            Status = x.Status.ToString(),
                            CanDelete =    x.Status == InterviewStatus.Created
                                        || x.Status == InterviewStatus.SupervisorAssigned
                                        || x.Status == InterviewStatus.InterviewerAssigned
                                        || x.Status == InterviewStatus.SentToCapi
                           
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
