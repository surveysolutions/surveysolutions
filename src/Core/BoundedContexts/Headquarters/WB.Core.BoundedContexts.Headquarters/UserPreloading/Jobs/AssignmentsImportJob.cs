using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Threading;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class AssignmentsImportJob : IJob
    {
        private ILogger logger => ServiceLocator.Current.GetInstance<ILoggerProvider>()
            .GetFor<AssignmentsImportJob>();
        
        private IAssignmentsImportService importAssignmentsService => ServiceLocator.Current
            .GetInstance<IAssignmentsImportService>();

        private IPlainTransactionManager plainTransactionManager => ServiceLocator.Current
            .GetInstance<IPlainTransactionManager>();

        private SampleImportSettings sampleImportSettings => ServiceLocator.Current
            .GetInstance<SampleImportSettings>();

        private IQuestionnaireStorage questionnaireStorage => ServiceLocator.Current
            .GetInstance<IQuestionnaireStorage>();

        private T ExecuteInPlain<T>(Func<T> func) => this.plainTransactionManager.ExecuteInPlainTransaction(func);
        private void ExecuteInPlain(Action func) => this.plainTransactionManager.ExecuteInPlainTransaction(func);

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var importProcess = this.ExecuteInPlain(() => this.importAssignmentsService.GetImportStatus());
                if (importProcess?.ProcessStatus != AssignmentsImportProcessStatus.Import) return;

                var allAssignmentIds = this.ExecuteInPlain(() => this.importAssignmentsService.GetAllAssignmentIdsToImport());

                this.logger.Debug("Assignments import job: Started");
                var sw = new Stopwatch();
                sw.Start();

                Parallel.ForEach(allAssignmentIds,
                    new ParallelOptions { MaxDegreeOfParallelism = this.sampleImportSettings.InterviewsImportParallelTasksLimit },
                    assignmentId =>
                    {
                        try
                        {
                            ThreadMarkerManager.MarkCurrentThreadAsIsolated();

                            var questionnaire = this.ExecuteInPlain(() => this.questionnaireStorage.GetQuestionnaire(importProcess.QuestionnaireIdentity, null));
                            if (questionnaire == null)
                            {
                                this.ExecuteInPlain(() => this.importAssignmentsService.RemoveAssignmentToImport(assignmentId));
                                return;
                            }

                            this.ImportAssignment(assignmentId, importProcess.AssignedTo, questionnaire);
                            this.ExecuteInPlain(() => this.importAssignmentsService.RemoveAssignmentToImport(assignmentId));

                        }
                        finally
                        {
                            ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                        }
                    });
                
                this.ExecuteInPlain(() =>
                {
                    importProcess = this.importAssignmentsService.GetImportStatus();

                    if (importProcess.WithErrorsCount == 0)
                        this.importAssignmentsService.RemoveAllAssignmentsToImport();
                    else
                        this.importAssignmentsService.SetImportProcessStatus(AssignmentsImportProcessStatus.ImportCompleted);
                });

                sw.Stop();
                this.logger.Debug($"Assignments import job: Finished. Elapsed time: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                this.logger.Error($"Assignments import job: FAILED. Reason: {ex.Message} ", ex);
            }
        }

        private void ImportAssignment(int assignmentId, Guid defaultResponsible, IQuestionnaire questionnaire)
        {
            var transactionManager = ServiceLocator.Current.GetInstance<ITransactionManagerProvider>().GetTransactionManager();

            try
            {
                transactionManager.BeginCommandTransaction();
                this.ExecuteInPlain(() => this.importAssignmentsService.ImportAssignment(assignmentId, defaultResponsible, questionnaire));
                transactionManager.CommitCommandTransaction();
            }
            catch
            {
                transactionManager.RollbackCommandTransaction();
            }
            
        }
    }
}
