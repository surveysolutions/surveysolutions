using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.BoundedContexts.Headquarters.Mappings;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;
using WB.Tests.Integration.PostgreSQLTests;

namespace WB.Tests.Integration.AudioAudit
{
    internal class AudioAuditStorageTests : with_postgres_db
    {
        protected ISessionFactory readFactory;
        protected ISessionFactory plainFactory;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            InitializeDb(DbType.PlainStore, DbType.ReadSide);

            readFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString,
              new List<Type>
              {
                    typeof(InterviewSummaryMap),
                    typeof(QuestionnaireCompositeItemMap),
                    typeof(IdentifyEntityValueMap),
                    typeof(InterviewStatisticsReportRowMap),
                    typeof(TimeSpanBetweenStatusesMap),
                    typeof(InterviewGpsMap),
                    typeof(CumulativeReportStatusChangeMap),
                    typeof(InterviewCommentedStatusMap),
                    typeof(InterviewCommentMap),
                    typeof(AudioAuditFileMap)
              }, true, new UnitOfWorkConnectionSettings().ReadSideSchemaName);

            plainFactory = IntegrationCreate.SessionFactory(ConnectionStringBuilder.ConnectionString,
              new List<Type>
              {
                    typeof(AudioAuditFileMap),
              }, true, new UnitOfWorkConnectionSettings().PlainStorageSchemaName);
        }

        private QuestionnaireIdentity questionnaireId = Create.Entity.QuestionnaireIdentity();

        [SetUp]
        public void Setup()
        {
            using var plain = IntegrationCreate.UnitOfWork(plainFactory);
            using var read = IntegrationCreate.UnitOfWork(readFactory);

            var auditStorageAccesor = new PostgresPlainStorageRepository<AudioAuditFile>(plain);
            var summary = IntegrationCreate.PostgresReadSideRepository<InterviewSummary>(read);
            var items = new List<InterviewSummary>
            {
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId.QuestionnaireId, questionnaireVersion: questionnaireId.Version),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId.QuestionnaireId, questionnaireVersion: questionnaireId.Version),
            };

            foreach (var item in items)
            {
                summary.Store(item, item.SummaryId);

                auditStorageAccesor.Store(new AudioAuditFile
                {
                    InterviewId = item.InterviewId,
                    ContentType = "text/plain",
                    FileName = "system1",
                    Id = $"{item.InterviewId}"
                }, $"{item.InterviewId}");
            }

            read.AcceptChanges();
            plain.AcceptChanges();
        }

        [Test]
        public async Task Should_HasAnyAudioAuditFilesStoredAsync_return_true_if_there_is_audit_files()
        {
            using var plain = IntegrationCreate.UnitOfWork(plainFactory);

            var auditStorageAccesor = new PostgresPlainStorageRepository<AudioAuditFile>(plain);
            var auditFileStorage = new AudioAuditFileStorage(auditStorageAccesor, plain);

            var hasAuditFiles = await auditFileStorage.HasAnyAudioAuditFilesStoredAsync(this.questionnaireId);

            Assert.True(hasAuditFiles);
        }

        [Test]
        public async Task Should_HasAnyAudioAuditFilesStoredAsync_return_false_if_there_is_no_audit_files()
        {
            using var plain = IntegrationCreate.UnitOfWork(plainFactory);

            var auditStorageAccesor = new PostgresPlainStorageRepository<AudioAuditFile>(plain);
            var auditFileStorage = new AudioAuditFileStorage(auditStorageAccesor, plain);

            var hasAuditFiles = await auditFileStorage.HasAnyAudioAuditFilesStoredAsync(Create.Entity.QuestionnaireIdentity());

            Assert.False(hasAuditFiles);
        }

        protected void ExecuteInCommandTransaction(Action action)
        {
            action();
        }
    }
}
