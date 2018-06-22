using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class QuestionnaireDownloadViewModel : MvxNotifyPropertyChanged
    {
        private readonly IPrincipal principal;
        private readonly IDesignerApiService designerApiService;
        private readonly ICommandService commandService;
        private readonly IQuestionnaireImportService questionnaireImportService;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IUserInteractionService userInteractionService;
        private readonly ILogger logger;
        private readonly IExecutedCommandsStorage executedCommandsStorage;
        private readonly IAttachmentContentStorage attachmentContentStorage;
        private readonly IFriendlyErrorMessageService friendlyErrorMessageService;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public QuestionnaireDownloadViewModel(
            IPrincipal principal,
            IDesignerApiService designerApiService, 
            ICommandService commandService, 
            IQuestionnaireImportService questionnaireImportService,
            IViewModelNavigationService viewModelNavigationService,
            IFriendlyErrorMessageService friendlyErrorMessageService,
            IUserInteractionService userInteractionService,
            ILogger logger,
            IExecutedCommandsStorage executedCommandsStorage,
            IAttachmentContentStorage attachmentContentStorage, 
            IQuestionnaireStorage questionnaireRepository)
        {
            this.principal = principal;
            this.designerApiService = designerApiService;
            this.commandService = commandService;
            this.questionnaireImportService = questionnaireImportService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.friendlyErrorMessageService = friendlyErrorMessageService;
            this.userInteractionService = userInteractionService;
            this.logger = logger;
            this.executedCommandsStorage = executedCommandsStorage;
            this.attachmentContentStorage = attachmentContentStorage;
            this.questionnaireRepository = questionnaireRepository;
        }

        public async Task<bool> LoadQuestionnaireAsync(string questionnaireId, string questionnaireTitle,
            IProgress<string> progress, CancellationToken cancellationToken)
        {
            var questionnaireIdentity = await DownloadQuestionnaireWithAllDependencisAsync(questionnaireId, questionnaireTitle, progress, cancellationToken).ConfigureAwait(false);
            if (questionnaireIdentity != null)
            {
                var interviewId = await this.CreateInterview(questionnaireIdentity, progress).ConfigureAwait(false);
                var questionnaire = this.questionnaireRepository.GetQuestionnaire(questionnaireIdentity, null);
                if (questionnaire.GetPrefilledQuestions().Count == 0)
                {
                    await this.viewModelNavigationService.NavigateToInterviewAsync(interviewId.FormatGuid(), null)
                        .ConfigureAwait(false);
                }
                else
                {
                    await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(interviewId.FormatGuid())
                        .ConfigureAwait(false);
                }
            }

            return questionnaireIdentity != null;
        }

        public async Task<bool> ReloadQuestionnaireAsync(string questionnaireId, string questionnaireTitle,
            IStatefulInterview interview, NavigationIdentity navigationIdentity, IProgress<string> progress, CancellationToken cancellationToken)
        {
            var questionnaireIdentity = await DownloadQuestionnaireWithAllDependencisAsync(questionnaireId, questionnaireTitle, progress, cancellationToken);
            if (questionnaireIdentity != null)
            {
                try
                {
                    progress.Report(TesterUIResources.ImportQuestionnaire_CreateInterview);

                    var interviewId = Guid.NewGuid();
                    
                    var existingInterviewCommands = this.executedCommandsStorage.Get(interview.Id);
                    foreach (var existingInterviewCommand in existingInterviewCommands)
                    {
                        if (existingInterviewCommand is CreateInterview createCommand)
                        {
                            createCommand.QuestionnaireId = questionnaireIdentity;
                        }
                        existingInterviewCommand.InterviewId = interviewId;
                        await this.commandService.ExecuteAsync(existingInterviewCommand, cancellationToken: cancellationToken);
                    }

                    if (navigationIdentity.TargetScreen == ScreenType.Identifying)
                    {
                        await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(interviewId.FormatGuid());
                    }
                    else
                    {
                        await this.viewModelNavigationService.NavigateToInterviewAsync(interviewId.FormatGuid(), navigationIdentity);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Cant reload questionnaire with data", e);
                    await userInteractionService.AlertAsync(TesterUIResources.ReloadInterviewErrorMessage);
                    var newInterviewId = await this.CreateInterview(questionnaireIdentity, progress);
                    await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(newInterviewId.FormatGuid());
                }
                finally
                { 
                    this.executedCommandsStorage.Clear(interview.Id);
                }
            }

            return questionnaireIdentity != null;
        }

        private async Task<QuestionnaireIdentity> DownloadQuestionnaireWithAllDependencisAsync(string questionnaireId, string questionnaireTitle,
            IProgress<string> progress, CancellationToken cancellationToken)
        {
            progress.Report(TesterUIResources.ImportQuestionnaire_CheckConnectionToServer);

            try
            {
                var questionnairePackage = await this.DownloadQuestionnaire(questionnaireId, progress, cancellationToken);

                if (questionnairePackage != null)
                {
                    progress.Report(TesterUIResources.ImportQuestionnaire_StoreQuestionnaire);

                    await this.DownloadQuestionnaireAttachments(questionnairePackage, progress, cancellationToken);

                    var dummyQuestionnaireIdentity = GenerateDummyQuestionnaireIdentity(questionnaireId);

                    var translations = await this.designerApiService.GetTranslationsAsync(questionnaireId, cancellationToken);

                    this.StoreQuestionnaireWithNewIdentity(dummyQuestionnaireIdentity, questionnairePackage, translations, progress);

                    return dummyQuestionnaireIdentity;
                }
            }
            catch (RestException ex)
            {
                if (ex.Type == RestExceptionType.RequestCanceledByUser)
                    return null;

                string errorMessage;
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Forbidden:
                        errorMessage = string.Format(TesterUIResources.ImportQuestionnaire_Error_Forbidden, questionnaireTitle);
                        break;
                    case HttpStatusCode.PreconditionFailed:
                        errorMessage = String.Format(TesterUIResources.ImportQuestionnaire_Error_PreconditionFailed, questionnaireTitle);
                        break;
                    case HttpStatusCode.NotFound:
                        errorMessage = String.Format(TesterUIResources.ImportQuestionnaire_Error_NotFound, questionnaireTitle);
                        break;
                    default:
                        errorMessage = this.friendlyErrorMessageService.GetFriendlyErrorMessageByRestException(ex);
                        break;
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    await this.userInteractionService.AlertAsync(errorMessage);
                else
                    throw;
            }
            catch (Exception ex)
            {
                this.logger.Error("Import questionnaire exception. ", ex);
            }

            return null;
        }

        private static QuestionnaireIdentity GenerateDummyQuestionnaireIdentity(string questionnaireId)
            => new QuestionnaireIdentity(Guid.Parse(questionnaireId), 1);

        private async Task<Guid> CreateInterview(QuestionnaireIdentity questionnaireIdentity, IProgress<string> progress)
        {
            progress.Report(TesterUIResources.ImportQuestionnaire_CreateInterview);

            var interviewId = Guid.NewGuid();

            await this.commandService.ExecuteAsync(new CreateInterview(
                interviewId: interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                questionnaireId: questionnaireIdentity,
                answers: new List<InterviewAnswer>(), 
                protectedVariables: new List<string>(), 
                answersTime: DateTime.UtcNow,
                supervisorId: Guid.NewGuid(),
                interviewerId: Guid.NewGuid(),
                interviewKey: null,
                assignmentId: null));

            return interviewId;
        }

        private void StoreQuestionnaireWithNewIdentity(QuestionnaireIdentity questionnaireIdentity,
            Questionnaire questionnairePackage, TranslationDto[] translations,
            IProgress<string> progress)
        {
            progress.Report(TesterUIResources.ImportQuestionnaire_StoreQuestionnaire);

            var questionnaireDocument = questionnairePackage.Document;
            questionnaireDocument.PublicKey = questionnaireIdentity.QuestionnaireId;
            questionnaireDocument.Id = questionnaireIdentity.QuestionnaireId.FormatGuid();

            var supportingAssembly = questionnairePackage.Assembly;

            this.questionnaireImportService.ImportQuestionnaire(questionnaireIdentity, questionnaireDocument, supportingAssembly, translations);
        }

        private async Task<Questionnaire> DownloadQuestionnaire(string questionnaireId,
            IProgress<string> progress, CancellationToken cancellationToken)
        {
            return await this.designerApiService.GetQuestionnaireAsync(
                questionnaireId: questionnaireId,
                onDownloadProgressChanged: downloadProgress => progress.Report(string.Format(TesterUIResources.ImportQuestionnaire_DownloadProgress, downloadProgress)),
                token: cancellationToken);
        }

        private async Task DownloadQuestionnaireAttachments(Questionnaire questionnaire,
            IProgress<string> progress, CancellationToken cancellationToken)
        {
            if (questionnaire == null)
                return;

            var attachments = questionnaire.Document.Attachments;

            foreach (var attachment in attachments)
            {
                var attachmentContentId = attachment.ContentId;

                var isExistsContent = this.attachmentContentStorage.Exists(attachmentContentId);
                if (!isExistsContent)
                {
                    var attachmentContent = await this.designerApiService.GetAttachmentContentAsync(
                        attachmentContentId,
                        onDownloadProgressChanged: downloadProgress => progress.Report(string.Format(TesterUIResources.ImportQuestionnaireAttachments_DownloadProgress, downloadProgress)),
                        token: cancellationToken);

                    this.attachmentContentStorage.Store(attachmentContent);
                }
            }
        }
    }
}
