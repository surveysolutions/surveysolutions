#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Main.Core.Documents;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.Questionnaire;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate
{
    internal class DeleteQuestionnaireService : IDeleteQuestionnaireService
    {
        private readonly IInterviewsToDeleteFactory interviewsToDeleteFactory;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader;
        private readonly ICommandService commandService;
        private readonly ILogger<DeleteQuestionnaireService> logger;
        private readonly ITranslationManagementService translations;
        private readonly IAssignmentsImportService importService;
        private readonly ISystemLog auditLog;

        private static readonly object DeleteInProcessLockObject = new object();
        private static readonly HashSet<string> DeleteInProcess = new HashSet<string>();
        private readonly IInvitationsDeletionService invitationsDeletionService;
        private readonly IAggregateRootCache aggregateRootCache;
        private readonly IAssignmentsToDeleteFactory assignmentsToDeleteFactory;
        private readonly IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage;
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IScheduledTask<DeleteQuestionnaireJob, DeleteQuestionnaireRequest> deleteQuestionnaireTask;

        private readonly IPlainKeyValueStorage<QuestionnaireBackup> questionnaireBackupStorage;

        public DeleteQuestionnaireService(IInterviewsToDeleteFactory interviewsToDeleteFactory,
            ICommandService commandService,
            ILogger<DeleteQuestionnaireService> logger,
            ITranslationManagementService translations,
            IAssignmentsImportService importService,
            ISystemLog auditLog,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemReader,
            IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage,
            IQuestionnaireStorage questionnaireStorage,
            IScheduledTask<DeleteQuestionnaireJob, DeleteQuestionnaireRequest> deleteQuestionnaireTask,
            IInvitationsDeletionService invitationsDeletionService,
            IAggregateRootCache aggregateRootCache,
            IAssignmentsToDeleteFactory assignmentsToDeleteFactory,
            IReusableCategoriesStorage reusableCategoriesStorage, IPlainKeyValueStorage<QuestionnaireBackup> questionnaireBackupStorage)
        {
            this.interviewsToDeleteFactory = interviewsToDeleteFactory;
            this.commandService = commandService;
            this.logger = logger;
            this.translations = translations;
            this.importService = importService;
            this.auditLog = auditLog;
            this.questionnaireBrowseItemReader = questionnaireBrowseItemReader;
            this.lookupTablesStorage = lookupTablesStorage;
            this.questionnaireStorage = questionnaireStorage;
            this.deleteQuestionnaireTask = deleteQuestionnaireTask;
            this.invitationsDeletionService = invitationsDeletionService;
            this.aggregateRootCache = aggregateRootCache;
            this.assignmentsToDeleteFactory = assignmentsToDeleteFactory;
            this.reusableCategoriesStorage = reusableCategoriesStorage;
            this.questionnaireBackupStorage = questionnaireBackupStorage;
        }

        public async Task DisableQuestionnaire(Guid questionnaireId, long questionnaireVersion, Guid userId)
        {
            this.logger.LogWarning("Questionnaire {questionnaireId}${questionnaireVersion} deletion was triggered by {userId} user", questionnaireId, questionnaireVersion, userId);
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            var questionnaire = this.questionnaireBrowseItemReader.GetById(questionnaireIdentity.ToString());

            if (questionnaire != null)
            {
                this.auditLog.QuestionnaireDeleted(questionnaire.Title, questionnaireIdentity);

                if (!questionnaire.Disabled)
                {
                    this.commandService.Execute(new DisableQuestionnaire(questionnaireId, questionnaireVersion,
                        userId));
                }

                await this.deleteQuestionnaireTask.Schedule(new DeleteQuestionnaireRequest
                {
                    QuestionnaireId = questionnaireId,
                    UserId = userId,
                    Version = questionnaireVersion
                });
            }
        }

        public async Task DeleteInterviewsAndQuestionnaireAfterAsync(Guid questionnaireId, long questionnaireVersion, Guid userId)
        {
            var questionnaireKey = ObjectExtensions.AsCompositeKey(questionnaireId.FormatGuid(), questionnaireVersion);

            lock (DeleteInProcessLockObject)
            {
                if (DeleteInProcess.Contains(questionnaireKey))
                    return;

                DeleteInProcess.Add(questionnaireKey);
            }

            var sw = Stopwatch.StartNew();
            string? title = null;
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);

            try
            {
            
                var questionnaireDocument = questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
                
                if (questionnaireDocument == null)
                    throw new ArgumentException($"questionnaire not found {questionnaireIdentity}");
                title = questionnaireDocument.Title;

                this.logger.LogWarning("Deletion of questionnaire {title} {id} started",
                    title, questionnaireIdentity);

                await this.DeleteInterviewsAsync(questionnaireIdentity);
                this.DeleteTranslations(questionnaireId, questionnaireVersion);
                this.DeleteLookupTables(questionnaireIdentity, questionnaireDocument);
                this.DeleteReusableCategories(questionnaireIdentity, questionnaireDocument);
                this.DeleteQuestionnaireBackup(questionnaireIdentity);

                var assignmentsImportStatus = this.importService.GetImportStatus();

                var isAssignmentImportIsGoing =
                    assignmentsImportStatus?.ProcessStatus == AssignmentsImportProcessStatus.Verification ||
                    assignmentsImportStatus?.ProcessStatus == AssignmentsImportProcessStatus.Import;

                if (!isAssignmentImportIsGoing)
                {
                    this.DeleteInvitations(questionnaireIdentity);
                    await this.DeleteAssignmentsAsync(questionnaireIdentity);
                    this.commandService.Execute(new DeleteQuestionnaire(questionnaireId, questionnaireVersion, userId));
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
            }
            finally
            {
                sw.Stop();
                this.logger.LogInformation("Questionnaire {title} {id} deleted in {seconds:0.00}s",
                    title ?? string.Empty, questionnaireIdentity, sw.Elapsed.TotalSeconds);
            }

            lock (DeleteInProcessLockObject)
            {
                DeleteInProcess.Remove(questionnaireKey);
            }
        }

        private void DeleteQuestionnaireBackup(QuestionnaireIdentity questionnaireIdentity)
        {
            questionnaireBackupStorage.Remove(questionnaireIdentity.ToString());
        }

        private void DeleteInvitations(QuestionnaireIdentity questionnaireIdentity)
        {
            invitationsDeletionService.Delete(questionnaireIdentity);
        }

        private async Task DeleteAssignmentsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await assignmentsToDeleteFactory.RemoveAllAssignmentsDataAsync(questionnaireIdentity);
            aggregateRootCache.Clear();
        }

        private void DeleteLookupTables(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument)
        {
            foreach (var lookupTableInfo in questionnaireDocument.LookupTables)
            {
                var id = lookupTablesStorage.GetLookupKey(questionnaireIdentity, lookupTableInfo.Key);
                lookupTablesStorage.Remove(id);
            }
        }

        private void DeleteReusableCategories(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument)
        {
            reusableCategoriesStorage.RemoveCategories(questionnaireIdentity);
        }

        private void DeleteTranslations(Guid questionnaireId, long questionnaireVersion)
        {
            this.translations.Delete(new QuestionnaireIdentity(questionnaireId, questionnaireVersion));
        }

        private async Task DeleteInterviewsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await interviewsToDeleteFactory.RemoveAllInterviewsDataAsync(questionnaireIdentity);
            aggregateRootCache.Clear();
        }
    }
}
