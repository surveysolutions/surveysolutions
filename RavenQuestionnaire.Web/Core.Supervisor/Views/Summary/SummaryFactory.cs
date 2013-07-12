using System.Collections.Generic;
using Main.Core.Entities;
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
            IEnumerable<SummaryItem> items = Enumerable.Empty<SummaryItem>();
           /* return this.summary.Query(
                _ =>
                {*/
                    if (input.ViewerStatus == ViewerStatus.Headquarter)
                    {
                        items = summary.QueryAll(x => x.ResponsibleSupervisorId == null);
                    }
                    else if (input.ViewerStatus == ViewerStatus.Supervisor)
                    {
                        items = summary.QueryAll(x => x.ResponsibleSupervisorId == input.ViewerId);
                    }

                    if (input.TemplateId.HasValue)
                    {
                        items = summary.QueryAll(x => x.TemplateId == input.TemplateId);
                    }

                    var all = items.GroupBy(
                        x => x.ResponsibleId,
                        y => y,
                        (x, y) =>
                            new SummaryViewItem()
                            {
                                User = new UserLight(x, y.FirstOrDefault().ResponsibleName),
                                Template = new TemplateLight(y.FirstOrDefault().TemplateId, string.Empty),
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
                        ItemsSummary = new SummaryViewItem()
                        {
                            User = new UserLight(Guid.Empty, "Summary"),
                            Total = all.Sum(x => x.Total),
                            Initial = all.Sum(x => x.Initial),
                            Error = all.Sum(x => x.Error),
                            Completed = all.Sum(x => x.Completed),
                            Approved = all.Sum(x => x.Approved),
                            Redo = all.Sum(x => x.Redo),
                            Unassigned = all.Sum(x => x.Unassigned)
                        }
                    };
               // });
        }
    }
}
