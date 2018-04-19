using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
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
                if (importProcess == null) return;

                if (importProcess.AssignedToInterviewersCount + importProcess.AssignedToSupervisorsCount == 0)
                    return;

                if (importProcess.VerifiedAssignments != importProcess.TotalAssignments)
                    return;

                if (importProcess.AssingmentsWithErrors == 0 && importProcess.TotalAssignments == 0)
                    this.ExecuteInPlain(() => this.importAssignmentsService.RemoveAllAssignmentsToImport());

                var allAssignmentIds = this.ExecuteInPlain(() => this.importAssignmentsService.GetAllAssignmentIdsToImport());
                if (allAssignmentIds.Length == 0) return;

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

                            this.ImportAssignment(assignmentId, questionnaire);
                            this.ExecuteInPlain(() => this.importAssignmentsService.RemoveAssignmentToImport(assignmentId));

                        }
                        finally
                        {
                            ThreadMarkerManager.ReleaseCurrentThreadFromIsolation();
                        }
                    });

                sw.Stop();
                this.logger.Debug($"Assignments import job: Finished. Elapsed time: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                this.logger.Error($"Assignments import job: FAILED. Reason: {ex.Message} ", ex);
            }
        }

        private void ImportAssignment(int assignmentId, IQuestionnaire questionnaire)
        {
            var transactionManager = ServiceLocator.Current.GetInstance<ITransactionManagerProvider>().GetTransactionManager();

            try
            {
                transactionManager.BeginCommandTransaction();
                this.ExecuteInPlain(() => this.importAssignmentsService.ImportAssignment(assignmentId, questionnaire));
                transactionManager.CommitCommandTransaction();
            }
            catch
            {
                transactionManager.RollbackCommandTransaction();
            }
            
        }
    }
}
