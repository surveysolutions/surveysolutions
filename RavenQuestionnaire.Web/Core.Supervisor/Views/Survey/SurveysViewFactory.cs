using System.Collections.Generic;
using Core.Supervisor.DenormalizerStorageItem;
using Core.Supervisor.RavenIndexes;
using Main.Core.Entities;
using Raven.Client.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Linq;

    using Main.Core.Utility;
    using Main.Core.View;

    public class SurveysViewFactory : IViewFactory<SurveysInputModel, SurveysView>
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public SurveysViewFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public SurveysView Load(SurveysInputModel input)
        {
            var items = Enumerable.Empty<SummaryItem>().AsQueryable();

            if (input.ViewerStatus == ViewerStatus.Supervisor)
            {
                items =
                    indexAccessor.Query<SummaryItem>(typeof(SupervisorReportsSurveysAndStatusesGroupByTeamMember).Name)
                                 .Where(x => x.ResponsibleSupervisorId == input.ViewerId);
            }
            else
            {

                items = indexAccessor.Query<SummaryItem>(typeof(HeadquarterReportsSurveysAndStatusesGroupByTeam).Name);
            }

            if (input.UserId.HasValue)
            {
              /*  if (input.ViewerStatus == ViewerStatus.Headquarter)
                {
                    items = items.Where(x => x.ResponsibleId == input.UserId);
                }
                else
                {*/
                    items = items.Where(x => x.ResponsibleId == input.UserId);
               // }

            }
            else
            {
                items = items.Where(x => x.ResponsibleId == Guid.Empty);
            }

            var all =
                items.OrderUsingSortExpression(input.Order)
                     .Skip((input.Page - 1)*input.PageSize)
                     .Take(input.PageSize)
                     .ToList()
                     .Select(y => new SurveysViewItem()
                         {
                             Template = new TemplateLight(y.TemplateId, y.TemplateName),
                             Approved = y.ApprovedCount,
                             Completed = y.CompletedCount,
                             Error = y.CompletedWithErrorsCount,
                             Initial = y.InitialCount,
                             Redo = y.RedoCount,
                             Unassigned = y.UnassignedCount,
                             Total = y.TotalCount
                         });

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
