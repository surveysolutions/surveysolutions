﻿using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
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
                pm.Type<PostgresSqlArrayType<string>>();
                pm.Column(clm => clm.SqlType("text[]"));
            });

            Property(x => x.ReferenceValues, pm =>
            {
                pm.Type<PostgresSqlArrayType<string>>();
                pm.Column(clm => clm.SqlType("text[]"));
            });

            Property(x => x.ReferenceValues, pm =>
            {
                pm.Type<PostgresSqlArrayType<string>>();
                pm.Column(clm => clm.SqlType("text[]"));
            });


            Property(x => x.SystemVariableValues, pm =>
            {
                pm.Type<PostgresSqlArrayType<string>>();
                pm.Column(clm => clm.SqlType("text[]"));
            });
            Property(x => x.Answers, pm =>
            {
                pm.Type<PostgresSqlArrayType<string>>();
                pm.Column(clm => clm.SqlType("text[]"));
            });

        }
    }

}