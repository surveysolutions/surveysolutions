using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Npgsql;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    public class InterviewsErrorsReader
    {
        private readonly PostgreConnectionSettings connectionSettings;
        private readonly string queryText;

        protected InterviewsErrorsReader()
        {
        }

        public InterviewsErrorsReader(PostgreConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
            this.queryText = $@"SELECT interviewid, entityid, rostervector, entitytype, invalidvalidations as FailedValidationConditions
                          FROM {connectionSettings.SchemaName}.interviews
                          WHERE interviewid = ANY(@interviews) AND entitytype IN(2, 3) AND array_length(invalidvalidations, 1) > 0
                          ORDER BY interviewid";
        }

        public virtual List<ExportedError> GetErrors(List<Guid> interveiws)
        {
            using (var connection = new NpgsqlConnection(connectionSettings.ConnectionString))
            {
                var array = interveiws.ToArray();
                var errors = connection.Query<ExportedError>(
                    queryText,
                    new
                    {
                        interviews = array
                    }).ToList();

                return errors;
            }
        }
    }
}