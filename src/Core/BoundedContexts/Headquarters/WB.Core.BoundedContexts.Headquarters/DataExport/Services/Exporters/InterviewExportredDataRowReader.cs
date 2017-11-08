using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NHibernate;
using NHibernate.Impl;
using Ninject;
using Npgsql;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    public class InterviewExportredDataRowReader
    {
        private readonly PostgreConnectionSettings connectionSettings;

        protected InterviewExportredDataRowReader()
        {
        }

        public InterviewExportredDataRowReader(PostgreConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        public virtual IList<InterviewDataExportRecord> ReadExportDataForInterview(Guid interviewId)
        {
            List<InterviewDataExportRecord> records;

            using (var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                records = connection.Query<InterviewDataExportRecord>($"SELECT * FROM {this.connectionSettings.SchemaName}.interviewdataexportrecords WHERE interviewid = @interviewId",
                                                                              new { interviewId = interviewId })
                                            .ToList();
            }

            return records;
        }
    }
}