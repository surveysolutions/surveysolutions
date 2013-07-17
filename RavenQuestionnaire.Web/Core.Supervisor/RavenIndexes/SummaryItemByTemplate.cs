using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.DenormalizerStorageItem;
using Main.Core.Entities.SubEntities;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Core.Supervisor.RavenIndexes
{
    public class SummaryItemByTemplate : AbstractMultiMapIndexCreationTask<SummaryItem>
    {
        public SummaryItemByTemplate()
        {
            AddMap<SummaryItem>(docs => from doc in docs
                                        where doc.ResponsibleSupervisorId!=null
                                        select new
                                        {
                                            doc.ResponsibleId,
                                            doc.ResponsibleName,
                                            doc.TemplateId,
                                            doc.TemplateName,
                                            doc.UnassignedCount,
                                            doc.InitialCount,
                                            doc.RedoCount,
                                            doc.CompletedCount,
                                            doc.CompletedWithErrorsCount,
                                            doc.ApprovedCount,
                                            doc.TotalCount,
                                            doc.ResponsibleSupervisorId
                                        });

            AddMap<SummaryItem>(docs => from doc in docs
                                        where doc.ResponsibleSupervisorId != null
                                        select new
                                        {
                                            ResponsibleId = Guid.Empty,
                                            doc.ResponsibleName,
                                            doc.TemplateId,
                                            doc.TemplateName,
                                            doc.UnassignedCount,
                                            doc.InitialCount,
                                            doc.RedoCount,
                                            doc.CompletedCount,
                                            doc.CompletedWithErrorsCount,
                                            doc.ApprovedCount,
                                            doc.TotalCount,
                                            doc.ResponsibleSupervisorId
                                        });

            Reduce = results => from result in results
                                group result by new { result.ResponsibleId, result.TemplateId } into g
                                select new
                                {
                                    ResponsibleId = g.Key.ResponsibleId,
                                    ResponsibleName = g.First().ResponsibleName,
                                    TemplateId = g.Key.TemplateId,
                                    TemplateName = g.First().TemplateName,
                                    UnassignedCount = g.Sum(x => x.UnassignedCount),
                                    InitialCount = g.Sum(x => x.InitialCount),
                                    RedoCount = g.Sum(x => x.RedoCount),
                                    CompletedCount = g.Sum(x => x.CompletedCount),
                                    CompletedWithErrorsCount = g.Sum(x => x.CompletedWithErrorsCount),
                                    ApprovedCount = g.Sum(x => x.ApprovedCount),
                                    TotalCount = g.Sum(x => x.TotalCount),
                                    ResponsibleSupervisorId = g.First().ResponsibleSupervisorId
                                };

            Index(x => x.ResponsibleSupervisorId, FieldIndexing.Analyzed);
            Index(x => x.TemplateId, FieldIndexing.Analyzed);
        }

    }
}
