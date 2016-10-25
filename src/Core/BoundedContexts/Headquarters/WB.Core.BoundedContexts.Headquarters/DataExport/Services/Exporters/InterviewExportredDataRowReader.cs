using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NHibernate;
using NHibernate.Impl;
using Ninject;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    public class InterviewExportredDataRowReader
    {
        private readonly ISessionFactory sessionFactory;

        protected InterviewExportredDataRowReader()
        {
        }

        public InterviewExportredDataRowReader([Named(PostgresReadSideModule.ReadSideSessionFactoryName)]ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public virtual IList<InterviewDataExportRecord> ReadExportDataForInterview(Guid interviewId)
        {
            IList<InterviewDataExportRecord> records = null;

            var schemaName = ((SessionFactoryImpl) sessionFactory).Settings.DefaultSchemaName;

            using (var session = this.sessionFactory.OpenStatelessSession())
            {
                records = session.Connection.Query<InterviewDataExportRecord>($"SELECT * FROM {schemaName}.interviewdataexportrecords WHERE interviewid = @interviewId",
                                                                              new { interviewId = interviewId })
                                            .ToList();
            }

            return records;
        }
    }
}