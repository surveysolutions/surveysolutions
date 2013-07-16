using System.Collections.Generic;
using Core.Supervisor.RavenIndexes;
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
            var items =
                summary.QueryWithIndex<SummaryItem>(typeof (SummaryItemByInterviewer));

            if (input.ViewerStatus == ViewerStatus.Headquarter)
            {
                items = items.Where(x => x.ResponsibleSupervisorId == null);
            }
            else if (input.ViewerStatus == ViewerStatus.Supervisor)
            {
                items = items.Where(x => x.ResponsibleSupervisorId == input.ViewerId);
            }
            
            if (input.TemplateId.HasValue)
            {
                items = items.Where(x => x.TemplateId == input.TemplateId);
            }
            else
            {
                items = items.Where(x => x.TemplateId != Guid.Empty);
            }

            var all = items.OrderUsingSortExpression(input.Order).Skip((input.Page - 1) * input.PageSize)
                           .Take(input.PageSize).ToList().Select(y =>
                                                                         new SummaryViewItem()
                                                                             {
                                                                                 User =
                                                                                     new UserLight(y.ResponsibleId,
                                                                                                   y.ResponsibleName),
                                                                                 Template =
                                                                                     new TemplateLight(y.TemplateId,
                                                                                                       string.Empty),
                                                                                 Approved = y.ApprovedCount,
                                                                                 Completed = y.CompletedCount,
                                                                                 Error = y.CompletedWithErrorsCount,
                                                                                 Initial = y.InitialCount,
                                                                                 Redo = y.RedoCount,
                                                                                 Unassigned = y.UnassignedCount,
                                                                                 Total = y.TotalCount
                                                                             }).ToList();

            return new SummaryView()
                {
                    TotalCount = all.Count(),
                    Items =
                        all,
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
        }
    }
}
