using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Mapping
{
    [PlainStorage]
    public class ExportedDataReferenceDtoMap : ClassMapping<ExportedDataReferenceDto>
    {
        public ExportedDataReferenceDtoMap()
        {
            Id(x => x.ExportedDataReferenceId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);
            Property(x => x.DataExportFormat);
            Property(x => x.DataExportType);
            Property(x => x.ExportedDataPath);
            Property(x => x.CreationDate);
            Property(x => x.FinishDate);
            Property(x => x.DataExportProcessId);
        }
    }
}