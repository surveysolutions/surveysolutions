using System.Collections.Generic;
using Core.Supervisor.DenormalizerStorageItem;
using Core.Supervisor.RavenIndexes;
using Main.Core.Entities;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Linq;

    using Main.Core.Utility;
    using Main.Core.View;

    public class SurveysViewFactory : IViewFactory<SurveysInputModel, SurveysView>
    {
        private readonly IQueryableReadSideRepositoryReader<SummaryItem> _summary;

        public SurveysViewFactory(IQueryableReadSideRepositoryReader<SummaryItem> summary)
        {
            this._summary = summary;
        }

        public SurveysView Load(SurveysInputModel input)
        {
            var items =
                _summary.QueryWithIndex<SummaryItem>(typeof (SummaryItemByTemplate));
      
                    /*if (input.ViewerStatus == ViewerStatus.Headquarter)
                    {
                        items = items.Where(x => x.ResponsibleSupervisorId == null);
                    }
                    else*/ if (input.ViewerStatus == ViewerStatus.Supervisor)
                    {
                        items = items.Where(x => x.ResponsibleSupervisorId == input.ViewerId);
                    }

                    if (input.UserId.HasValue)
                    {
                        if (input.ViewerStatus == ViewerStatus.Headquarter)
                        {
                            items = items.Where(x => x.ResponsibleSupervisorId == input.UserId && x.ResponsibleId == Guid.Empty);
                        }
                        else
                        {
                            items = items.Where(x => x.ResponsibleId == input.UserId);
                        }
                        
                    }
                    else
                    {
                        items = items.Where(x => x.ResponsibleId == Guid.Empty);
                    }

                /*    var all = items.ToList().GroupBy(
                        x => x.TemplateId,
                        y => y,
                        (x, y) =>
                            new SurveysViewItem()
                            {
                                Template = new TemplateLight(x, y.FirstOrDefault().TemplateName),
                                Approved = y.Sum(z => z.ApprovedCount),
                                Completed = y.Sum(z => z.CompletedCount),
                                Error = y.Sum(z => z.CompletedWithErrorsCount),
                                Initial = y.Sum(z => z.InitialCount),
                                Redo = y.Sum(z => z.RedoCount),
                                Unassigned = y.Sum(z => z.UnassignedCount),
                                Total = y.Sum(z => z.TotalCount)
                            }).AsQueryable().OrderUsingSortExpression(input.Order);*/
                    var all = items.OrderUsingSortExpression(input.Order).Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).Select(y => new SurveysViewItem()
                {
                    Template = new TemplateLight(y.TemplateId, y.TemplateName),
                    Approved = y.ApprovedCount,
                    Completed = y.CompletedCount,
                    Error = y.CompletedWithErrorsCount,
                    Initial = y.InitialCount,
                    Redo = y.RedoCount,
                    Unassigned = y.UnassignedCount,
                    Total = y.TotalCount
                }).ToList();

                    return new SurveysView()
                    {
                        TotalCount = all.Count(),
                        Items =
                            all,
                        ItemsSummary = new SurveysViewItem(
                            new TemplateLight(Guid.Empty, "Summary"),
                            all.Sum(x => x.Total),
                            all.Sum(x => x.Initial),
                            all.Sum(x => x.Error),
                            all.Sum(x => x.Completed),
                            all.Sum(x => x.Approved),
                            all.Sum(x => x.Redo),
                            all.Sum(x => x.Unassigned))
                    };
               // });
        }
    }
}
