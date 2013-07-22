using System;
using System.Linq.Expressions;
using Core.Supervisor.Views.Survey;
using Main.Core.Entities;
using Main.Core.Utility;
using Raven.Client.Linq;
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
            /*  return this.interviews.Query(_ =>
              {*/
            Expression<Func<InterviewItem, bool>> predicate = (s) => true;

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
                if (input.OnlyNotAssigned)
                {
                    predicate = predicate.AndCondition(t => t.ResponsibleSupervisorId == null);
                }
                else if (input.ResponsibleId.HasValue)
                {
                    predicate = predicate.AndCondition(x => x.ResponsibleSupervisorId == input.ResponsibleId);
                }
            }
            else
            {
                if (input.OnlyNotAssigned)
                {
                    predicate = predicate.AndCondition(x => x.Responsible.Id == input.ViewerId);
                }
                else if (input.ResponsibleId.HasValue)
                {
                    predicate = predicate.AndCondition(x => x.Responsible.Id == input.ResponsibleId);
                }
                else
                {

                    predicate =
                        predicate.AndCondition(
                            x => x.ResponsibleSupervisorId != null && x.ResponsibleSupervisorId == input.ViewerId);
                }
            }

            //var items = DefineOrderBy(_.ToList().AsQueryable(), input)
            //    .Skip((input.Page - 1)*input.PageSize)
            //    .Take(input.PageSize);

            var interviewItems = DefineOrderBy(this.interviews.QueryEnumerable(predicate),input)
                            .Skip((input.Page - 1)*input.PageSize)
                            .Take(input.PageSize).ToList();

           
            return new InterviewView()
                {
                    TotalCount = this.interviews.Count(predicate),
                    Items = interviewItems.Select(x => new InterviewViewItem()
                        {
                            FeaturedQuestions = x.FeaturedQuestions,
                            InterviewId = x.InterviewId,
                            LastEntryDate = x.LastEntryDate.ToShortDateString(),
                            Responsible = x.Responsible,
                            Status = x.Status.Name,
                            Title = x.Title
                        })
                };
            //  });
        }

        private IQueryable<InterviewItem> DefineOrderBy(IQueryable<InterviewItem> query,
                                                        InterviewInputModel model)
        {
            var orderBy = model.Orders.FirstOrDefault();
            if (orderBy == null)
            {
                return query;
            }
            /*  List<string> o = query.SelectMany(t => t.FeaturedQuestions).Select(y => y.Question).Distinct().ToList();
                if (o.Contains(orderBy.Field))
                {
                    query = orderBy.Direction == OrderDirection.Asc
                        ? query.OrderBy(
                            t =>
                                t.FeaturedQuestions.Where(y => y.Question == orderBy.Field).Select(
                                    x => x.Answer).FirstOrDefault())
                        : query.OrderByDescending(
                            t =>
                                t.FeaturedQuestions.Where(y => y.Question == orderBy.Field).Select(
                                    x => x.Answer).FirstOrDefault());
                }
                else
                {
                    query =*/
            return query.OrderUsingSortExpression(model.Order).AsQueryable();
            //   }
        }
    }
}
