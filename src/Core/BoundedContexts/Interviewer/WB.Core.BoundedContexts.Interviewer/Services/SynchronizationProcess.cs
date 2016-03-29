using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public class SynchronizationProcess : ISynchronizationProcess
    {
        private readonly ISynchronizationService synchronizationService;
        private bool shouldUpdatePasswordOfInterviewer;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IAsyncPlainStorage<InterviewView> interviewViewRepository;
        private readonly IInterviewerInterviewAccessor interviewFactory;

        private readonly IPrincipal principal;
        private readonly ILogger logger;
        private readonly IUserInteractionService userInteractionService;
        private readonly IInterviewerQuestionnaireAccessor questionnaireFactory;
        private readonly IAttachmentContentStorage attachmentContentStorage;
        private readonly IAsyncPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage;
        private readonly IAsyncPlainStorage<InterviewFileView> interviewFileViewStorage;
        private readonly IPasswordHasher passwordHasher;

        private RestCredentials restCredentials;

        public SynchronizationProcess(ISynchronizationService synchronizationService, 
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage, 
            IAsyncPlainStorage<InterviewView> interviewViewRepository,
            IPrincipal principal,
            ILogger logger, 
            IUserInteractionService userInteractionService, 
            IInterviewerQuestionnaireAccessor questionnaireFactory, 
            IAttachmentContentStorage attachmentContentStorage, 
            IInterviewerInterviewAccessor interviewFactory, 
            IAsyncPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage, 
            IAsyncPlainStorage<InterviewFileView> interviewFileViewStorage,
            IPasswordHasher passwordHasher)
        {
            this.synchronizationService = synchronizationService;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.interviewViewRepository = interviewViewRepository;
            this.principal = principal;
            this.logger = logger;
            this.userInteractionService = userInteractionService;
            this.questionnaireFactory = questionnaireFactory;
            this.attachmentContentStorage = attachmentContentStorage;
            this.interviewFactory = interviewFactory;
            this.interviewMultimediaViewStorage = interviewMultimediaViewStorage;
            this.interviewFileViewStorage = interviewFileViewStorage;
            this.passwordHasher = passwordHasher;
        }

        public async Task SyncronizeAsync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            SychronizationStatistics statistics = new SychronizationStatistics();
            try
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_UserAuthentication_Title ,
                    Description = InterviewerUIResources.Synchronization_UserAuthentication_Description,
                    Status = SynchronizationStatus.Started,
                    Statistics = statistics
                });

                this.restCredentials = this.restCredentials ?? new RestCredentials {
                    Login = this.principal.CurrentUserIdentity.Name,
                    Password = this.principal.CurrentUserIdentity.Password
                };
                await this.synchronizationService.CanSynchronizeAsync(token: cancellationToken, credentials: restCredentials);

                if (this.shouldUpdatePasswordOfInterviewer)
                {
                    this.shouldUpdatePasswordOfInterviewer = false;
                    await this.UpdatePasswordOfInterviewerAsync(restCredentials.Password);
                }

                await this.UploadCompletedInterviewsAsync(statistics, progress, cancellationToken);
                await this.DownloadCensusAsync(progress, cancellationToken);
                await this.DownloadInterviewsAsync(statistics, progress, cancellationToken);

                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Success_Title,
                    Description = InterviewerUIResources.Synchronization_Success_Description,
                    Status = SynchronizationStatus.Success,
                    Statistics = statistics
                });
            }
            catch (SynchronizationException ex)
            {
                var errorTitle = InterviewerUIResources.Synchronization_Fail_Title;
                var errorDescription = ex.Message;

                switch (ex.Type)
                {
                    case SynchronizationExceptionType.RequestCanceledByUser:
                        progress.Report(new SyncProgressInfo {
                            Status = SynchronizationStatus.Stopped,
                            Statistics = statistics
                        });
                        break;
                    case SynchronizationExceptionType.Unauthorized:
                        this.shouldUpdatePasswordOfInterviewer = true;
                        break;
                    case SynchronizationExceptionType.UserLinkedToAnotherDevice:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = InterviewerUIResources.Synchronization_UserLinkedToAnotherDevice_Status,
                            Description = InterviewerUIResources.Synchronization_UserLinkedToAnotherDevice_Title,
                            UserIsLinkedToAnotherDevice = true,
                            Status = SynchronizationStatus.Stopped,
                            Statistics = statistics
                        });
                        break;
                }

                var status = ex.Type == SynchronizationExceptionType.RequestCanceledByUser
                    ? SynchronizationStatus.Canceled
                    : SynchronizationStatus.Fail;
                progress.Report(new SyncProgressInfo
                {
                    Title = errorTitle,
                    Description = errorDescription,
                    Status = status,
                    Statistics = statistics
                });
            }
            catch (Exception ex)
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Fail_Title,
                    Description = InterviewerUIResources.Synchronization_Fail_UnexpectedException,
                    Status = SynchronizationStatus.Fail,
                    Statistics = statistics
                });

                this.logger.Error("Synchronization. Unexpected exception", ex);
            }

            if (!cancellationToken.IsCancellationRequested && this.shouldUpdatePasswordOfInterviewer)
            {
                var newPassword = await this.GetNewPasswordAsync();
                if (newPassword == null)
                {
                    this.shouldUpdatePasswordOfInterviewer = false;
                }
                else
                {
                    this.restCredentials.Password = this.passwordHasher.Hash(newPassword);
                    await this.SyncronizeAsync(progress, cancellationToken);
                }
            }
        }

        private async Task<string> GetNewPasswordAsync()
        {
            return await this.userInteractionService.ConfirmWithTextInputAsync(
                message: InterviewerUIResources.Synchronization_UserPassword_Update_Format.FormatString(this.principal.CurrentUserIdentity.Name),
                okButton: UIResources.LoginText,
                cancelButton: InterviewerUIResources.Synchronization_Cancel,
                isTextInputPassword: true);
        }

        private async Task DownloadCensusAsync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            var remoteCensusQuestionnaireIdentities = await this.synchronizationService.GetCensusQuestionnairesAsync(cancellationToken);

            var localCensusQuestionnaireIdentities = this.questionnaireFactory.GetCensusQuestionnaireIdentities();

            var notExistingRemoteCensusQuestionnaireIdentities = localCensusQuestionnaireIdentities.Where(
                questionnaireIdentity => !remoteCensusQuestionnaireIdentities.Contains(questionnaireIdentity));

            foreach (var censusQuestionnaireIdentity in notExistingRemoteCensusQuestionnaireIdentities)
            {
                await this.questionnaireFactory.RemoveQuestionnaireAsync(censusQuestionnaireIdentity);
            }

            var processedQuestionnaires = 0;
            var notExistingLocalCensusQuestionnaireIdentities = remoteCensusQuestionnaireIdentities.Except(localCensusQuestionnaireIdentities).ToList();
            foreach (var censusQuestionnaireIdentity in notExistingLocalCensusQuestionnaireIdentities)
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Download_Title,
                    Description = string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                        processedQuestionnaires,
                        notExistingLocalCensusQuestionnaireIdentities.Count,
                        InterviewerUIResources.Synchronization_Questionnaires)
                });
                await this.DownloadQuestionnaireAsync(censusQuestionnaireIdentity, cancellationToken);

                processedQuestionnaires++;
            }
        }

        private async Task DownloadQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, CancellationToken cancellationToken)
        {
            if (!this.questionnaireFactory.IsQuestionnaireAssemblyExists(questionnaireIdentity))
            {
                var questionnaireAssembly = await this.synchronizationService.GetQuestionnaireAssemblyAsync(
                    questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: cancellationToken);

                await this.questionnaireFactory.StoreQuestionnaireAssemblyAsync(questionnaireIdentity, questionnaireAssembly);
                await this.synchronizationService.LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(questionnaireIdentity);
            }

            if (!this.questionnaireFactory.IsQuestionnaireExists(questionnaireIdentity))
            {
                var contentIds = await this.synchronizationService.GetAttachmentContentsAsync(questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: cancellationToken);

                foreach (var contentId in contentIds)
                {
                    var isExistContent = await this.attachmentContentStorage.IsExistAsync(contentId);
                    if (!isExistContent)
                    {
                        var attachmentContent = await this.synchronizationService.GetAttachmentContentAsync(contentId: contentId,
                            onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                            token: cancellationToken);

                        await this.attachmentContentStorage.StoreAsync(attachmentContent);
                    }
                }


                var questionnaireApiView = await this.synchronizationService.GetQuestionnaireAsync(
                    questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: cancellationToken);

                await this.questionnaireFactory.StoreQuestionnaireAsync(questionnaireIdentity,
                    questionnaireApiView.QuestionnaireDocument, questionnaireApiView.AllowCensus);

                await this.synchronizationService.LogQuestionnaireAsSuccessfullyHandledAsync(questionnaireIdentity);
            }
        }

        private async Task DownloadInterviewsAsync(SychronizationStatistics statistics, IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            var remoteInterviews = await this.synchronizationService.GetInterviewsAsync(cancellationToken);

            var remoteInterviewIds = remoteInterviews.Select(interview => interview.Id);

            var localInterviews = this.interviewViewRepository.LoadAll();

            var localInterviewIds = localInterviews.Select(interview => interview.InterviewId).ToList();

            var localInterviewsToRemove = localInterviews.Where(
                interview => !remoteInterviewIds.Contains(interview.InterviewId) && !interview.CanBeDeleted);

            var localInterviewIdsToRemove = localInterviewsToRemove.Select(interview => interview.InterviewId).ToList();

            var remoteInterviewsToCreate = remoteInterviews.Where(interview => !localInterviewIds.Contains(interview.Id)).ToList();

            await this.RemoveInterviewsAsync(localInterviewIdsToRemove, statistics, progress);

            await this.CreateInterviewsAsync(remoteInterviewsToCreate, statistics, progress, cancellationToken);
        }

        private async Task RemoveInterviewsAsync(List<Guid> interviewIds, SychronizationStatistics statistics, IProgress<SyncProgressInfo> progress)
        {
            statistics.TotalDeletedInterviewsCount = interviewIds.Count;

            foreach (var interviewId in interviewIds)
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Download_Title,
                    Description = string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                        statistics.DeletedInterviewsCount + 1,
                        interviewIds.Count,
                        InterviewerUIResources.Synchronization_Interviews)
                });

                await this.interviewFactory.RemoveInterviewAsync(interviewId);

                statistics.DeletedInterviewsCount++;
            }
        }

        private async Task CreateInterviewsAsync(List<InterviewApiView> interviews, 
            SychronizationStatistics statistics,
            IProgress<SyncProgressInfo> progress, 
            CancellationToken cancellationToken)
        {
            statistics.TotalNewInterviewsCount = interviews.Count(interview => !interview.IsRejected);
            statistics.TotalRejectedInterviewsCount = interviews.Count(interview => interview.IsRejected);

            foreach (var interview in interviews)
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Download_Title,
                    Description = string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                        statistics.RejectedInterviewsCount + statistics.NewInterviewsCount + 1, interviews.Count,
                        InterviewerUIResources.Synchronization_Interviews)
                });

                await this.DownloadQuestionnaireAsync(interview.QuestionnaireIdentity, cancellationToken);

                var interviewDetails = await this.synchronizationService.GetInterviewDetailsAsync(
                    interviewId: interview.Id,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: cancellationToken);

                await this.interviewFactory.CreateInterviewAsync(interview, interviewDetails);
                await this.synchronizationService.LogInterviewAsSuccessfullyHandledAsync(interview.Id);

                if (interview.IsRejected)
                    statistics.RejectedInterviewsCount++;
                else
                    statistics.NewInterviewsCount++;
            }
        }

        private async Task UploadCompletedInterviewsAsync(SychronizationStatistics statistics, IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            var completedInterviews = this.interviewViewRepository.Where(interview => interview.Status == InterviewStatus.Completed);

            statistics.TotalCompletedInterviewsCount = completedInterviews.Count;

            foreach (var completedInterview in completedInterviews)
            {
                var completedInterviewApiView = await this.interviewFactory.GetPackageByCompletedInterviewAsync(completedInterview.InterviewId);
                progress.Report(new SyncProgressInfo
                {
                    Title = string.Format(InterviewerUIResources.Synchronization_Upload_Title_Format, InterviewerUIResources.Synchronization_Upload_CompletedAssignments_Text),
                    Description = string.Format(InterviewerUIResources.Synchronization_Upload_Description_Format,
                        statistics.CompletedInterviewsCount, statistics.TotalCompletedInterviewsCount,
                        InterviewerUIResources.Synchronization_Upload_Interviews_Text)
                });

                await this.UploadImagesByCompletedInterview(completedInterview.InterviewId, progress, cancellationToken);

                await this.synchronizationService.UploadInterviewAsync(
                    interviewId: completedInterview.InterviewId,
                    completedInterview: completedInterviewApiView,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: cancellationToken);

                await this.interviewFactory.RemoveInterviewAsync(completedInterview.InterviewId);

                statistics.CompletedInterviewsCount++;
            }
        }

        private async Task UploadImagesByCompletedInterview(Guid interviewId, IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            var imageViews = this.interviewMultimediaViewStorage.Where(image => image.InterviewId == interviewId);

            foreach (var imageView in imageViews)
            {
                var fileView = await this.interviewFileViewStorage.GetByIdAsync(imageView.FileId);
                await this.synchronizationService.UploadInterviewImageAsync(
                    interviewId: imageView.InterviewId,
                    fileName: imageView.FileName,
                    fileData: fileView.File,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: cancellationToken);
                await this.interviewMultimediaViewStorage.RemoveAsync(imageView.Id);
                await this.interviewFileViewStorage.RemoveAsync(fileView.Id);
            }
        }

        private async Task UpdatePasswordOfInterviewerAsync(string password)
        {
            var localInterviewer = this.interviewersPlainStorage.FirstOrDefault();
            localInterviewer.Password = password;

            await this.interviewersPlainStorage.StoreAsync(localInterviewer);
            await this.principal.SignInAsync(localInterviewer.Name, localInterviewer.Password, true);
        }
    }
}