using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NHibernate;
using Ninject;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
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

            using (var session = this.sessionFactory.OpenStatelessSession())
            {
                records = session.Connection.Query<InterviewDataExportRecord>("SELECT * FROM interviewdataexportrecords WHERE interviewid = @interviewId",
                                                                              new { interviewId = interviewId })
                                            .ToList();
            }

            return records;
        }
    }
}