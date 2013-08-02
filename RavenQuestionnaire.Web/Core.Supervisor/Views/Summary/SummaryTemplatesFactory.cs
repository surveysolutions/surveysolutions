using System;
using System.Collections.Generic;
using Core.Supervisor.DenormalizerStorageItem;

namespace Core.Supervisor.Views.Summary
{
    using System.Linq;

    using Main.Core.View;

    using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

    public class SummaryTemplatesFactory : IViewFactory<SummaryTemplatesInputModel, SummaryTemplatesView>
    {
        private readonly IQueryableReadSideRepositoryReader<SummaryItem> summary;

        public SummaryTemplatesFactory(IQueryableReadSideRepositoryReader<SummaryItem> summary)
        {
            this.summary = summary;
        }

        public SummaryTemplatesView Load(SummaryTemplatesInputModel input)
        {
            return this.summary.Query(
                _ =>
                {
                    if (input.ViewerStatus == ViewerStatus.Headquarter)
                    {
                        _ = _.Where(x => x.ResponsibleSupervisorId == null);
                    }
                    else if (input.ViewerStatus == ViewerStatus.Supervisor)
                    {
                        _ =
                            _.Where(x => x.ResponsibleSupervisorId == input.ViewerId || x.ResponsibleId == input.ViewerId);
                    }
                    return new SummaryTemplatesView()
                    {
                        Items =
                            _.ToList().Distinct(new SummaryItemByTemplateNameComparer())
                                .Select(
                                    x =>
                                        new SummaryTemplateViewItem()
                                        {
                                            TemplateId =
                                                x.TemplateId,
                                            TemplateName =
                                                x.TemplateName
                                        })
                    };
                });
        }
    }

    public class SummaryItemByTemplateNameComparer : IEqualityComparer<SummaryItem>
    {
        public bool Equals(SummaryItem x, SummaryItem y)
        {
            if (Object.ReferenceEquals(y, null)) return false;

            if (Object.ReferenceEquals(x, y)) return true;

            return x.TemplateName.Equals(y.TemplateName);
        }

        public int GetHashCode(SummaryItem obj)
        {
            return obj.TemplateName.GetHashCode();
        }
    }
}
