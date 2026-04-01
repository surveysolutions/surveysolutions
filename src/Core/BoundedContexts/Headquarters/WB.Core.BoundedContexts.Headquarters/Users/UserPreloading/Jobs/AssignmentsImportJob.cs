using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Domain;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    [RetryFailedJob]
    internal class AssignmentsImportJob : IJob
    {
        private const int MaxImportAttempts = 3;

        private readonly IServiceLocator serviceLocator;
        private readonly ILogger logger;
        private readonly IAssignmentsImportService assignmentsImportService;
        private readonly ISystemLog systemLog;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IInScopeExecutor inScopeExecutor;

        public AssignmentsImportJob(IServiceLocator serviceLocator, ILogger logger,
            IAssignmentsImportService assignmentsImportService, ISystemLog systemLog,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IInScopeExecutor inScopeExecutor)
        {
            this.serviceLocator = serviceLocator;
            this.logger = logger;
            this.assignmentsImportService = assignmentsImportService;
            this.systemLog = systemLog;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.inScopeExecutor = inScopeExecutor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var sampleImportSettings = serviceLocator.GetInstance<SampleImportSettings>();

            AssignmentsImportStatus importProcessStatus = assignmentsImportService.GetImportStatus();
            if (importProcessStatus?.ProcessStatus != AssignmentsImportProcessStatus.Import)
                return;

            var allAssignmentIds = assignmentsImportService.GetAllAssignmentIdsToImport();

            importProcessStatus = assignmentsImportService.GetImportStatus();
            if (importProcessStatus.ProcessStatus != AssignmentsImportProcessStatus.Import)
                return;

            var userManager = serviceLocator.GetInstance<IUserRepository>();
            var responsibleId = (await userManager
                .FindByNameAsync(importProcessStatus.ResponsibleName).ConfigureAwait(false)).Id;

            this.logger.Debug("Assignments import job: Started");
            var sw = new Stopwatch();
            sw.Start();

            int? lastImportedAssignmentId = null;
            int? firstImportedAssignmentId = null;

            Parallel.ForEach(allAssignmentIds,
                new ParallelOptions { MaxDegreeOfParallelism = sampleImportSettings.InterviewsImportParallelTasksLimit },
                assignmentId =>
                {
                    Exception lastException = null;

                    for (int attempt = 0; attempt < MaxImportAttempts; attempt++)
                    {
                        try
                        {
                            inScopeExecutor.Execute((serviceLocatorLocal) =>
                            {
                                var threadImportAssignmentsService = serviceLocatorLocal.GetInstance<IAssignmentsImportService>();

                                var questionnaire = serviceLocatorLocal.GetInstance<IQuestionnaireStorage>().GetQuestionnaire(importProcessStatus.QuestionnaireIdentity, null);
                                if (questionnaire == null)
                                {
                                    threadImportAssignmentsService.RemoveAssignmentToImport(assignmentId);
                                    return;
                                }

                                var newAssignmentId = threadImportAssignmentsService.ImportAssignment(assignmentId,
                                    importProcessStatus.AssignedTo, questionnaire, responsibleId);

                                if (!firstImportedAssignmentId.HasValue)
                                    firstImportedAssignmentId = newAssignmentId;
                                else
                                    lastImportedAssignmentId = newAssignmentId;

                                threadImportAssignmentsService.RemoveAssignmentToImport(assignmentId);
                            });

                            lastException = null;
                            break;
                        }
                        catch (Exception ex)
                        {
                            lastException = ex;

                            if (attempt < MaxImportAttempts - 1)
                            {
                                this.logger.Warn($"Assignment {assignmentId} import attempt {attempt + 1} failed. Retrying. Reason: {ex.Message}");
                                Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                            }
                        }
                    }

                    if (lastException != null)
                    {
                        this.logger.Error($"Assignment {assignmentId} import failed after {MaxImportAttempts} attempts. Reason: {lastException.Message}", lastException);
                        inScopeExecutor.Execute((serviceLocatorLocal) =>
                            serviceLocatorLocal.GetInstance<IAssignmentsImportService>()
                                .SetVerifiedToAssignment(assignmentId, lastException.Message));
                    }
                });

            inScopeExecutor.Execute((serviceLocatorLocal) =>
                serviceLocatorLocal.GetInstance<IAssignmentsImportService>()
                    .SetImportProcessStatus(AssignmentsImportProcessStatus.ImportCompleted));

            var questionnaireTitle = this.questionnaireBrowseViewFactory.GetById(importProcessStatus.QuestionnaireIdentity).Title;
            var questionnaireVersion = importProcessStatus.QuestionnaireIdentity.Version;

            this.systemLog.AssignmentsImported(importProcessStatus.TotalCount, questionnaireTitle,
                questionnaireVersion, firstImportedAssignmentId ?? 0, lastImportedAssignmentId ?? firstImportedAssignmentId ?? 0, importProcessStatus.ResponsibleName);

            sw.Stop();
            this.logger.Debug($"Assignments import job: Finished. Elapsed time: {sw.Elapsed}");
        }
    }
}
