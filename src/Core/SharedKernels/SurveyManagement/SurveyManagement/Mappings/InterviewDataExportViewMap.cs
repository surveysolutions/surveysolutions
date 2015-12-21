using System.Collections.Generic;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.Storage.Postgre.NhExtensions;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class InterviewDataExportRecordMap : ClassMapping<InterviewDataExportRecord>
    {
        public InterviewDataExportRecordMap()
        {
            Id(x => x.Id, idMap => idMap.Generator(Generators.Assigned));

            Property(x => x.RecordId);
            Property(x => x.LevelName);
            Property(x => x.InterviewId, clm => clm.Index("Export_Record_InterviewId_indx"));

            Property(x => x.ParentRecordIds, pm =>
            {
                pm.Type<PostgresSqlStringArrayType>();
                pm.Column(clm => clm.SqlType("text[]"));
            });

            Property(x => x.ReferenceValues, pm =>
            {
                pm.Type<PostgresSqlStringArrayType>();
                pm.Column(clm => clm.SqlType("text[]"));
            });

            Property(x => x.ReferenceValues, pm =>
            {
                pm.Type<PostgresSqlStringArrayType>();
                pm.Column(clm => clm.SqlType("text[]"));
            });


            Property(x => x.SystemVariableValues, pm =>
            {
                pm.Type<PostgresSqlStringArrayType>();
                pm.Column(clm => clm.SqlType("text[]"));
            });
            Property(x => x.Answers, pm =>
            {
                pm.Type<PostgresSqlStringArrayType>();
                pm.Column(clm => clm.SqlType("text[]"));
            });

        }
    }

}