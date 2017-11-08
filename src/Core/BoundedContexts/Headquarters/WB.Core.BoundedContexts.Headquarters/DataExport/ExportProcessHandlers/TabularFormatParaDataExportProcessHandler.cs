﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class TabularFormatParaDataExportProcessHandler: AbstractDataExportHandler
    {
        private readonly IEventStore eventStore;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;
        private readonly IUserViewFactory userReader;

        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;

        private ITransactionManager TransactionManager => this.transactionManagerProvider.GetTransactionManager();
        private IPlainTransactionManager PlainTransactionManager => this.plainTransactionManagerProvider.GetPlainTransactionManager();

        private void ExecuteInTransaction(Action action) => this.TransactionManager.ExecuteInQueryTransaction(()
            => this.PlainTransactionManager.ExecuteInPlainTransaction(action.Invoke));

        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ICsvWriter csvWriter;
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly ILogger logger;

        public TabularFormatParaDataExportProcessHandler(IEventStore eventStore,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader,
            IUserViewFactory userReader,
            InterviewDataExportSettings interviewDataExportSettings,
            ITransactionManagerProvider transactionManagerProvider,
            IDataExportProcessesService dataExportProcessesService, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage, 
            IPlainTransactionManagerProvider plainTransactionManagerProvider,
            IQuestionnaireStorage questionnaireStorage,
            IFileSystemAccessor fs,
            IFilebasedExportedDataAccessor dataAccessor,
            IDataExportFileAccessor exportFileAccessor,
            ICsvWriter csvWriter,
            ITabularFormatExportService tabularFormatExportService,
            ILogger logger) : base(fs, dataAccessor, interviewDataExportSettings, dataExportProcessesService, exportFileAccessor)
        {
            this.eventStore = eventStore;
            this.interviewSummaryReader = interviewSummaryReader;
            this.userReader = userReader;
            this.transactionManagerProvider = transactionManagerProvider;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.questionnaireStorage = questionnaireStorage;
            this.csvWriter = csvWriter;
            this.tabularFormatExportService = tabularFormatExportService;
            this.logger = logger;
        }

        protected override DataExportFormat Format => DataExportFormat.Paradata;

        protected override void ExportDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, string directoryPath,
            IProgress<int> progress, CancellationToken cancellationToken)
        {
            var interviewsToExport = this.tabularFormatExportService.GetInterviewIdsToExport(
                questionnaireIdentity, status, cancellationToken);

            var paradataReader = new InMemoryReadSideRepositoryAccessor<InterviewHistoryView>();

            var interviewParaDataEventHandler = new InterviewParaDataEventHandler(paradataReader,
                this.interviewSummaryReader, this.userReader, this.interviewDataExportSettings,
                this.questionnaireExportStructureStorage, this.questionnaireStorage);

            cancellationToken.ThrowIfCancellationRequested();

            var exportFilePath = this.fileSystemAccessor.CombinePath(directoryPath, "paradata.tab");

            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(exportFilePath, true))
            using (var writer = this.csvWriter.OpenCsvWriter(fileStream, ExportFileSettings.DataFileSeparator.ToString()))
            {
                writer.WriteField("id");
                writer.WriteField("event");
                writer.WriteField("responsible");
                writer.WriteField("role");
                writer.WriteField("timestamp");
                writer.WriteField("parameters");
                writer.NextRecord();

                long totalInterviewsProcessed = 0;
                foreach (var interviewId in interviewsToExport)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var eventsByInterview = this.eventStore.Read(interviewId, 0);

                    try
                    {
                        this.ExecuteInTransaction(() => eventsByInterview.ForEach(interviewParaDataEventHandler.Handle));

                        var paradata = paradataReader.Query(_ => _.FirstOrDefault());
                        foreach (var evnt in paradata?.Records)
                        {
                            writer.WriteField(interviewId);
                            writer.WriteField(evnt.Action);
                            writer.WriteField(evnt.OriginatorName);
                            writer.WriteField(evnt.OriginatorRole);
                            writer.WriteField(evnt.Timestamp?.ToString("s", CultureInfo.InvariantCulture) ?? "");
                            foreach (var value in evnt.Parameters.Values)
                            {
                                writer.WriteField(value ?? string.Empty);
                            }
                            writer.NextRecord();
                        }

                        paradataReader.Clear();
                    }
                    catch (Exception e)
                    {
                        this.logger.Error($"Paradata unhandled exception for interview {interviewId}", e);
                    }

                    totalInterviewsProcessed++;
                    progress.Report(totalInterviewsProcessed.PercentOf(interviewsToExport.Count));
                }
            }
        }
    }
}