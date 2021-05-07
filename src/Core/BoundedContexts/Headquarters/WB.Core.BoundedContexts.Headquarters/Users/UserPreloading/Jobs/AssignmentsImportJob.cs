using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Domain;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class AssignmentsImportJob : IJob
    {
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
            try
            {
                var sampleImportSettings = serviceLocator.GetInstance<SampleImportSettings>();

                AssignmentsImportStatus importProcessStatus = assignmentsImportService.GetImportStatus();
                if (importProcessStatus?.ProcessStatus != AssignmentsImportProcessStatus.Import)
                    return;

                var allAssignmentIds = assignmentsImportService.GetAllAssignmentIdsToImport();
                
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
                        inScopeExecutor.Execute((serviceLocatorLocal) =>
                        {
                            var threadImportAssignmentsService = serviceLocatorLocal.GetInstance<IAssignmentsImportService>();

                            var questionnaire = serviceLocatorLocal.GetInstance<IQuestionnaireStorage>().GetQuestionnaire(importProcessStatus.QuestionnaireIdentity, null);
                            if (questionnaire == null)
                            {
                                threadImportAssignmentsService.RemoveAssignmentToImport(assignmentId);
                                return;
                            }

                            try
                            {
                                var newAssignmentId = threadImportAssignmentsService.ImportAssignment(assignmentId,
                                    importProcessStatus.AssignedTo, questionnaire, responsibleId);

                                if (!firstImportedAssignmentId.HasValue)
                                    firstImportedAssignmentId = newAssignmentId;
                                else
                                    lastImportedAssignmentId = newAssignmentId;

                                threadImportAssignmentsService.RemoveAssignmentToImport(assignmentId);
                            }
                            catch (Exception ex)
                            {
                                this.logger.Error($"Assignment import error. Reason: {ex.Message} ", ex);
                                threadImportAssignmentsService.SetVerifiedToAssignment(assignmentId, ex.Message);
                            }
                            
                        });
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
            catch (Exception ex)
            {
                this.logger.Error($"Assignments import job: FAILED. Reason: {ex.Message} ", ex);

                inScopeExecutor.Execute((serviceLocatorLocal) =>
                    serviceLocatorLocal.GetInstance<IAssignmentsImportService>()
                        .SetImportProcessStatus(AssignmentsImportProcessStatus.ImportCompleted));
            }
        }
    }
}
