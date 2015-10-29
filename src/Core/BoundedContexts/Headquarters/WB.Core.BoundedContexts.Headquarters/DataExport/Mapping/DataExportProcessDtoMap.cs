using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Mapping
{
    [PlainStorage]
    public class DataExportProcessDtoMap : ClassMapping<DataExportProcessDto>
    {
        public DataExportProcessDtoMap()
        {
            Id(x => x.DataExportProcessId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            Property(x => x.QuestionnaireId);
            Property(x => x.QuestionnaireVersion);
            Property(x => x.DataExportFormat);
            Property(x => x.DataExportType);
            Property(x => x.BeginDate);
            Property(x => x.LastUpdateDate);
            Property(x => x.ProgressInPercents);
            Property(x => x.Status);
        }
    }
}