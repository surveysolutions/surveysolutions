using Main.Core.Entities;
using Main.Core.Utility;
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
            return this.interviews.Query(_ =>
            {
                if (input.StatusId.HasValue)
                {
                    _ = _.Where(x => (x.Status.Id == input.StatusId));
                }

                if (input.TemplateId.HasValue)
                {
                    _ = _.Where(x => (x.TemplateId == input.TemplateId));
                }

                if (input.ViewerStatus == ViewerStatus.Headquarter)
                {
                    if (input.OnlyNotAssigned)
                    {
                        _ = _.Where(t => t.ResponsibleSupervisorId == null);
                    }
                    else if (input.ResponsibleId.HasValue)
                    {
                        _ = _.Where(x => x.ResponsibleSupervisorId == input.ResponsibleId);
                    }    
                }
                else
                {
                    _ = _.Where(x => x.ResponsibleSupervisorId != null && x.ResponsibleSupervisorId == input.ViewerId);

                    if (input.OnlyNotAssigned)
                    {
                        _ = _.Where(x=>x.Responsible.Id == input.ViewerId);
                    }
                    else if (input.ResponsibleId.HasValue)
                    {
                        _ = _.Where(x => x.Responsible.Id == input.ResponsibleId);
                    }    
                }

                var items = DefineOrderBy(_.ToList().AsQueryable(), input)
                    .Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize);

                return new InterviewView()
                {
                    TotalCount = _.Count(),
                    Items = items.Select(x => new InterviewViewItem()
                    {
                        FeaturedQuestions = x.FeaturedQuestions,
                        InterviewId = x.InterviewId,
                        LastEntryDate = x.LastEntryDate.ToShortDateString(),
                        Responsible = x.Responsible,
                        Status = x.Status.Name,
                        Title = x.Title
                    })
                };
            });
        }

        private IQueryable<InterviewItem> DefineOrderBy(IQueryable<InterviewItem> query,
            InterviewInputModel model)
        {
            var orderBy = model.Orders.FirstOrDefault();
            if (orderBy != null)
            {
                List<string> o = query.SelectMany(t => t.FeaturedQuestions).Select(y => y.Question).Distinct().ToList();
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
                    query = query.OrderUsingSortExpression(model.Order).AsQueryable();
                }
            }
            return query;
        }
    }
}