using System.Linq.Expressions;
using Core.Supervisor.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views.Summary
{
    using System;
    using System.Linq;

    using Core.Supervisor.DenormalizerStorageItem;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Utility;
    using Main.Core.View;

    public class SummaryFactory : IViewFactory<SummaryInputModel, SummaryView>
    {
        private readonly IQueryableReadSideRepositoryReader<SummaryItem> summary;

        public SummaryFactory(IQueryableReadSideRepositoryReader<SummaryItem> summary)
        {
            this.summary = summary;
        }

        public SummaryView Load(SummaryInputModel input)
        {
            Expression<Func<SummaryItem, bool>> predicate = (x) => true;

            if (input.ViewerStatus == ViewerStatus.Headquarter)
            {
                predicate = predicate.AndCondition(x => x.ResponsibleSupervisorId == null);
            }
            else if (input.ViewerStatus == ViewerStatus.Supervisor)
            {
                if (input.TemplateId.HasValue)
                {
                    predicate = predicate.AndCondition(x => x.TemplateId == input.TemplateId);
                }
                else
                {
                    predicate = predicate.AndCondition(x => x.ResponsibleSupervisorId == input.ViewerId);
                }
            }



            var all = summary.QueryAll(predicate).GroupBy(
                x => x.ResponsibleId,
                y => y,
                (x, y) =>
                new SummaryViewItem()
                    {
                        User = new UserLight(x, y.FirstOrDefault().ResponsibleName),
                        Approved = y.Sum(z => z.ApprovedCount),
                        Completed = y.Sum(z => z.CompletedCount),
                        Error = y.Sum(z => z.CompletedWithErrorsCount),
                        Initial = y.Sum(z => z.InitialCount),
                        Redo = y.Sum(z => z.RedoCount),
                        Unassigned = y.Sum(z => z.UnassignedCount),
                        Total = y.Sum(z => z.TotalCount)
                    }).AsQueryable().OrderUsingSortExpression(input.Order);

            return new SummaryView()
                {
                    TotalCount = all.Count(),
                    Items =
                        all.Skip((input.Page - 1)*input.PageSize)
                           .Take(input.PageSize),
                    ItemsSummary = new SummaryViewItem(
                        new UserLight(Guid.Empty, "Summary"),
                        all.Sum(x => x.Total),
                        all.Sum(x => x.Initial),
                        all.Sum(x => x.Error),
                        all.Sum(x => x.Completed),
                        all.Sum(x => x.Approved),
                        all.Sum(x => x.Redo),
                        all.Sum(x => x.Unassigned))
                };

        }
    }
}
