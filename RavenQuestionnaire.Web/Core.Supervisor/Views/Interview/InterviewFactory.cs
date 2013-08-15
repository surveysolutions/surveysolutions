using System;
using System.Linq.Expressions;
using Core.Supervisor.Views.Survey;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Raven.Client.Linq;
using Raven.Database.Linq.PrivateExtensions;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views.Interview
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.View;

    public class InterviewFactory : IViewFactory<InterviewInputModel, InterviewView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewItem> interviews;

        public InterviewFactory(
            IQueryableReadSideRepositoryReader<InterviewItem> interviews)
        {
            this.interviews = interviews;
        }

        public InterviewView Load(InterviewInputModel input)
        {
            Expression<Func<InterviewItem, bool>> predicate = (s) => !s.IsDeleted;

            if (input.StatusId.HasValue)
            {
                predicate = predicate.AndCondition(x => (x.Status.Id == input.StatusId));
            }

            if (input.TemplateId.HasValue)
            {
                predicate = predicate.AndCondition(x => (x.TemplateId == input.TemplateId));
            }

            if (input.ViewerStatus == ViewerStatus.Headquarter)
            {
                predicate = predicate.AndCondition(x => x.ResponsibleSupervisorId == input.ResponsibleId);
            }
            else
            {
                predicate = input.ResponsibleId.HasValue 
                    ? predicate.AndCondition(x => x.Responsible.Id == input.ResponsibleId) 
                    : predicate.AndCondition(x => x.ResponsibleSupervisorId != null && x.ResponsibleSupervisorId == input.ViewerId);
            }

            var interviewItems = DefineOrderBy(this.interviews.Query(_ => _.Where(predicate)), input)
                            .Skip((input.Page - 1) * input.PageSize)
                            .Take(input.PageSize).ToList();


            return new InterviewView()
                {
                    TotalCount = this.interviews.Query(_ => _.Count(predicate)),
                    Items = interviewItems.Select(x => new InterviewViewItem()
                        {
                            FeaturedQuestions = x.FeaturedQuestions,
                            InterviewId = x.InterviewId,
                            LastEntryDate = x.LastEntryDate.ToShortDateString(),
                            Responsible = x.Responsible,
                            Status = x.Status.Name,
                            Title = x.Title,
                            CanDelete = x.Status.Id == SurveyStatus.Unknown.PublicId ||
                                        x.Status.Id == SurveyStatus.Unassign.PublicId || x.Status.Id == SurveyStatus.Initial.PublicId,
                            CanBeReassigned = x.Status.Id == SurveyStatus.Unknown.PublicId || x.Status.Id == SurveyStatus.Redo.PublicId ||
                                        x.Status.Id == SurveyStatus.Unassign.PublicId || x.Status.Id == SurveyStatus.Initial.PublicId
                        })
                };
        }

        private IQueryable<InterviewItem> DefineOrderBy(IQueryable<InterviewItem> query,
                                                        InterviewInputModel model)
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
