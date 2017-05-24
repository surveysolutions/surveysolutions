using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using MvvmCross.Platform;
using MvvmCross.Platform.Exceptions;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Services.Synchronization;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public class SynchronizationProcess : ISynchronizationProcess
    {
        private readonly ISynchronizationService synchronizationService;
        private bool shouldUpdatePasswordOfInterviewer;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<AssignmentDocument> assignmentsRepository;
        private readonly IInterviewerInterviewAccessor interviewFactory;
        private readonly IPrincipal principal;
        private readonly ILogger logger;
        private readonly IUserInteractionService userInteractionService;
        private readonly IInterviewerQuestionnaireAccessor questionnairesAccessor;
        private readonly IAttachmentContentStorage attachmentContentStorage;
        private readonly IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage;
        private readonly IPlainStorage<InterviewFileView> interviewFileViewStorage;
        private readonly CompanyLogoSynchronizer logoSynchronizer;
        private readonly AttachmentsCleanupService cleanupService;
        private readonly IPasswordHasher passwordHasher;
        private readonly IHttpStatistician httpStatistician;

        private RestCredentials restCredentials;
        private bool remoteLoginRequired = false;

        public SynchronizationProcess(ISynchronizationService synchronizationService, 
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage, 
            IPlainStorage<InterviewView> interviewViewRepository,
            IPlainStorage<AssignmentDocument> assignmentsRepository,
            IPrincipal principal,
            ILogger logger, 
            IUserInteractionService userInteractionService, 
            IInterviewerQuestionnaireAccessor questionnairesAccessor, 
            IAttachmentContentStorage attachmentContentStorage, 
            IInterviewerInterviewAccessor interviewFactory, 
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage, 
            IPlainStorage<InterviewFileView> interviewFileViewStorage,
            CompanyLogoSynchronizer logoSynchronizer, 
            AttachmentsCleanupService cleanupService,
            IPasswordHasher passwordHasher,
            IHttpStatistician httpStatistician)
        {
            this.synchronizationService = synchronizationService;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.interviewViewRepository = interviewViewRepository;
            this.assignmentsRepository = assignmentsRepository;
            this.principal = principal;
            this.logger = logger;
            this.userInteractionService = userInteractionService;
            this.questionnairesAccessor = questionnairesAccessor;
            this.attachmentContentStorage = attachmentContentStorage;
            this.interviewFactory = interviewFactory;
            this.interviewMultimediaViewStorage = interviewMultimediaViewStorage;
            this.interviewFileViewStorage = interviewFileViewStorage;
            this.logoSynchronizer = logoSynchronizer;
            this.cleanupService = cleanupService;
            this.passwordHasher = passwordHasher;
            this.httpStatistician = httpStatistician;
        }

        public async Task SyncronizeAsync(IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            SychronizationStatistics statistics = new SychronizationStatistics();
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                
                this.httpStatistician.Reset();
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_UserAuthentication_Title,
                    Description = InterviewerUIResources.Synchronization_UserAuthentication_Description,
                    Status = SynchronizationStatus.Started,
                    Statistics = statistics
                });

                this.restCredentials = this.restCredentials ?? new RestCredentials
                {
                    Login = this.principal.CurrentUserIdentity.Name,
                    Token = this.principal.CurrentUserIdentity.Token
                };

                if (this.remoteLoginRequired)
                {
                    var token = await this.synchronizationService.LoginAsync(new LogonInfo
                    {
                        Username = this.restCredentials.Login,
                        Password = this.restCredentials.Password
                    }, this.restCredentials);

                    this.restCredentials.Password = this.restCredentials.Password;
                    this.restCredentials.Token = token;

                    this.remoteLoginRequired = false;

                    await this.synchronizationService.GetInterviewerAsync(this.restCredentials);
                }

                if (this.shouldUpdatePasswordOfInterviewer)
                {
                    this.shouldUpdatePasswordOfInterviewer = false;
                    this.UpdatePasswordOfInterviewer(this.restCredentials);
                }

                await this.synchronizationService.CanSynchronizeAsync(token: cancellationToken, credentials: this.restCredentials);
                
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    DeviceInfo deviceInfo = null;

                    using (var deviceInformationService = ServiceLocator.Current.GetInstance<IDeviceInformationService>())
                    {
                        deviceInfo = await deviceInformationService.GetDeviceInfoAsync();
                    }
                    
                    await this.synchronizationService.SendDeviceInfoAsync(this.ToDeviceInfoApiView(deviceInfo), cancellationToken);
                }
                catch (Exception e)
                {
                    await this.TrySendUnexpectedExceptionToServerAsync(e, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                await this.UploadCompletedInterviewsAsync(progress, statistics, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                await this.SyncronizeAssignmentsAsync(progress, statistics, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                await this.SyncronizeCensusQuestionnaires(progress, statistics, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                await this.CheckObsoleteQuestionnairesAsync(progress, statistics, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                await this.DownloadInterviewsAsync(statistics, progress, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                await this.logoSynchronizer.DownloadCompanyLogo(progress, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await this.synchronizationService.SendSyncStatisticsAsync(
                        statistics: ToSyncStatisticsApiView(statistics, stopwatch),
                        token: cancellationToken, credentials: this.restCredentials);
                }
                catch (Exception e)
                {
                    await this.TrySendUnexpectedExceptionToServerAsync(e, cancellationToken);
                }

                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Success_Title,
                    Description = InterviewerUIResources.Synchronization_Success_Description,
                    Status = SynchronizationStatus.Success,
                    Statistics = statistics
                });
            }
            catch (OperationCanceledException)
            {
                progress.Report(new SyncProgressInfo
                {
                    Status = SynchronizationStatus.Stopped,
                    Statistics = statistics
                });

                return;
            }
            catch (SynchronizationException ex)
            {
                var errorTitle = InterviewerUIResources.Synchronization_Fail_Title;
                var errorDescription = ex.Message;

                switch (ex.Type)
                {
                    case SynchronizationExceptionType.RequestCanceledByUser:
                        progress.Report(new SyncProgressInfo {
                            Title = errorTitle,
                            Description = errorDescription,
                            Status = SynchronizationStatus.Canceled,
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
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics
                        });
                        break;
                    case SynchronizationExceptionType.UnacceptableSSLCertificate:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = InterviewerUIResources.UnexpectedException,
                            Description = InterviewerUIResources.UnacceptableSSLCertificate,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics
                        });
                        break;
                    default:
                        progress.Report(new SyncProgressInfo
                        {
                            Title = errorTitle,
                            Description = errorDescription,
                            Status = SynchronizationStatus.Fail,
                            Statistics = statistics
                        });
                        break;
                }
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

                await this.TrySendUnexpectedExceptionToServerAsync(ex, cancellationToken);
                this.logger.Error("Synchronization. Unexpected exception", ex);
            }

            if (!cancellationToken.IsCancellationRequested && this.shouldUpdatePasswordOfInterviewer)
            {
                var newPassword = await this.GetNewPasswordAsync();
                if (newPassword == null)
                {
                    this.shouldUpdatePasswordOfInterviewer = false;
                    progress.Report(new SyncProgressInfo
                    {
                        Title = InterviewerUIResources.Synchronization_Fail_Title,
                        Description = InterviewerUIResources.Unauthorized,
                        Status = SynchronizationStatus.Fail,
                        Statistics = statistics
                    });
                }
                else
                {
                    this.remoteLoginRequired = true;
                    this.restCredentials.Password = newPassword;
                    await this.SyncronizeAsync(progress, cancellationToken);
                }
            }
        }

        private async Task TrySendUnexpectedExceptionToServerAsync(Exception exception, CancellationToken cancellationToken)
        {
            try
            {
                await this.synchronizationService.SendUnexpectedExceptionAsync(ToUnexpectedExceptionApiView(exception),
                    cancellationToken);
            }
            catch(Exception ex)
            {
                this.logger.Error("Synchronization. Exception when send exception to server", ex);
            }
        }

        private Task<string> GetNewPasswordAsync()
        {
            var message = InterviewerUIResources.Synchronization_UserPassword_Update_Format.FormatString(this.principal.CurrentUserIdentity.Name);
            return this.userInteractionService.ConfirmWithTextInputAsync(
                message: message,
                okButton: UIResources.LoginText,
                cancelButton: InterviewerUIResources.Synchronization_Cancel,
                isTextInputPassword: true);
        }

        private async Task SyncronizeAssignmentsAsync(IProgress<SyncProgressInfo> progress, SychronizationStatistics statistics, CancellationToken cancellationToken)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Of_Assignments,
                Statistics = statistics,
                Status = SynchronizationStatus.Download
            });
            
            var remoteAssignments = await this.synchronizationService.GetAssignmentsAsync(cancellationToken);
            var localAssignments = this.assignmentsRepository.LoadAll();

            // removing local assignments if needed
            var remoteIds = remoteAssignments.ToLookup(ra => ra.Id);
            foreach (var assignment in localAssignments)
            {
                if (remoteIds.Contains(assignment.Id)) continue;

                statistics.RemovedAssignmentsCount += 1;
                this.assignmentsRepository.Remove(assignment.Id);
            }

            // adding new, updating capacity for existing
            var localAssignmentsLookup = localAssignments.ToLookup(la => la.Id);
            foreach (var remote in remoteAssignments)
            {
                var local = localAssignmentsLookup[remote.Id].FirstOrDefault();

                if (local == null)
                {
                    local = new AssignmentDocument
                    {
                        Id = remote.Id,
                        QuestionnaireId = remote.QuestionnaireId,
                        Created = 0,
                        IdentifyingData = remote.IdentifyingData
                    };

                    statistics.NewAssignmentsCount += 1;
                    await this.DownloadQuestionnaireAsync(QuestionnaireIdentity.Parse(local.QuestionnaireId), cancellationToken, statistics);
                }

                local.Capacity = remote.Capacity;

                this.assignmentsRepository.Store(local);
            }
        }
        
        private async Task CheckObsoleteQuestionnairesAsync(IProgress<SyncProgressInfo> progress, SychronizationStatistics statistics, CancellationToken cancellationToken)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires,
                Statistics = statistics,
                Status = SynchronizationStatus.Download
            });

            var serverQuestionnaires = await this.synchronizationService.GetServerQuestionnairesAsync(cancellationToken);
            var localQuestionnaires = this.questionnairesAccessor.GetAllQuestionnaireIdentities();
            
            var questionnairesToRemove = localQuestionnaires.Except(serverQuestionnaires).ToList();
            
            int removedQuestionnairesCounter = 0;
            foreach (var questionnaireIdentity in questionnairesToRemove)
            {
                cancellationToken.ThrowIfCancellationRequested();
                removedQuestionnairesCounter++;
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires,
                    Description = string.Format(InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires_Description, 
                        removedQuestionnairesCounter, questionnairesToRemove.Count),
                    Statistics = statistics,
                    Status = SynchronizationStatus.Download
                });

                var questionnaireId = questionnaireIdentity.ToString();

                var removedInterviews = this.interviewViewRepository
                    .Where(interview => interview.QuestionnaireId == questionnaireId)
                    .Select(interview => interview.InterviewId)
                    .ToList();
                this.RemoveInterviews(removedInterviews, statistics, progress);

                this.questionnairesAccessor.RemoveQuestionnaire(questionnaireIdentity);
            }
            if (questionnairesToRemove.Count > 0)
            {
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Download_AttachmentsCleanup
                });
                this.cleanupService.RemovedOrphanedAttachments();
            }
        }

        private async Task SyncronizeCensusQuestionnaires(IProgress<SyncProgressInfo> progress, SychronizationStatistics statistics,
            CancellationToken cancellationToken)
        {
            var remoteCensusQuestionnaireIdentities = await this.synchronizationService.GetCensusQuestionnairesAsync(cancellationToken);
            var localCensusQuestionnaireIdentities = this.questionnairesAccessor.GetCensusQuestionnaireIdentities();

            var processedQuestionnaires = 0;
            var notExistingLocalCensusQuestionnaireIdentities = remoteCensusQuestionnaireIdentities.Except(localCensusQuestionnaireIdentities).ToList();

            foreach (var censusQuestionnaireIdentity in notExistingLocalCensusQuestionnaireIdentities)
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Download_Title,
                    Description = string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                        processedQuestionnaires,
                        notExistingLocalCensusQuestionnaireIdentities.Count,
                        InterviewerUIResources.Synchronization_Questionnaires)
                });

                await this.DownloadQuestionnaireAsync(censusQuestionnaireIdentity, cancellationToken, statistics);

                processedQuestionnaires++;
            }
        }

        private async Task DownloadQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, CancellationToken cancellationToken, SychronizationStatistics statistics)
        {
            if (!this.questionnairesAccessor.IsQuestionnaireAssemblyExists(questionnaireIdentity))
            {
                var questionnaireAssembly = await this.synchronizationService.GetQuestionnaireAssemblyAsync(
                    questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: cancellationToken);

                await this.questionnairesAccessor.StoreQuestionnaireAssemblyAsync(questionnaireIdentity, questionnaireAssembly);
                await this.synchronizationService.LogQuestionnaireAssemblyAsSuccessfullyHandledAsync(questionnaireIdentity);
            }

            if (!this.questionnairesAccessor.IsQuestionnaireExists(questionnaireIdentity))
            {
                var contentIds = await this.synchronizationService.GetAttachmentContentsAsync(questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: cancellationToken);

                foreach (var contentId in contentIds)
                {
                    var isExistContent = attachmentContentStorage.Exists(contentId);
                    if (!isExistContent)
                    {
                        var attachmentContent = await this.synchronizationService.GetAttachmentContentAsync(contentId: contentId,
                            onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                            token: cancellationToken); 

                        this.attachmentContentStorage.Store(attachmentContent);
                    }
                }

                List<TranslationDto> translationDtos = 
                    await this.synchronizationService.GetQuestionnaireTranslationAsync(questionnaireIdentity, cancellationToken);
                
                var questionnaireApiView = await this.synchronizationService.GetQuestionnaireAsync(
                    questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: cancellationToken);

                this.questionnairesAccessor.StoreQuestionnaire(questionnaireIdentity, questionnaireApiView.QuestionnaireDocument, 
                    questionnaireApiView.AllowCensus, translationDtos);

                await this.synchronizationService.LogQuestionnaireAsSuccessfullyHandledAsync(questionnaireIdentity);
                statistics.SuccessfullyDownloadedQuestionnairesCount++;
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

            this.RemoveInterviews(localInterviewIdsToRemove, statistics, progress);

            await this.CreateInterviewsAsync(remoteInterviewsToCreate, statistics, progress, cancellationToken);
        }

        private void RemoveInterviews(List<Guid> interviewIds, SychronizationStatistics statistics, IProgress<SyncProgressInfo> progress)
        {
            statistics.TotalDeletedInterviewsCount += interviewIds.Count;

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

                this.interviewFactory.RemoveInterview(interviewId);

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
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progress.Report(new SyncProgressInfo
                    {
                        Title = InterviewerUIResources.Synchronization_Download_Title,
                        Description = string.Format(InterviewerUIResources.Synchronization_Download_Description_Format,
                            statistics.RejectedInterviewsCount + statistics.NewInterviewsCount + 1, interviews.Count,
                            InterviewerUIResources.Synchronization_Interviews)
                    });

                    await this.DownloadQuestionnaireAsync(interview.QuestionnaireIdentity, cancellationToken, statistics);

                    var interviewDetails = await this.synchronizationService.GetInterviewDetailsAsync(
                        interviewId: interview.Id,
                        onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                        token: cancellationToken);

                    if (interviewDetails == null)
                    {
                        statistics.NewInterviewsCount ++;
                        continue;
                    }

                    await this.interviewFactory.CreateInterviewAsync(interview, interviewDetails);
                    await this.synchronizationService.LogInterviewAsSuccessfullyHandledAsync(interview.Id);

                    if (interview.IsRejected)
                        statistics.RejectedInterviewsCount++;
                    else
                        statistics.NewInterviewsCount++;
                }
                catch (Exception exception)
                {
                    statistics.FailedToCreateInterviewsCount++;

                    await this.TrySendUnexpectedExceptionToServerAsync(exception, cancellationToken);
                    this.logger.Error($"Failed to create interview {interview.Id}, interviewer {this.principal.CurrentUserIdentity.Name}", exception);
                }
            }
        }

        private async Task UploadCompletedInterviewsAsync(IProgress<SyncProgressInfo> progress, SychronizationStatistics statistics, CancellationToken cancellationToken)
        {
            var completedInterviews = this.interviewViewRepository.Where(interview => interview.Status == InterviewStatus.Completed);

            statistics.TotalCompletedInterviewsCount = completedInterviews.Count;

            foreach (var completedInterview in completedInterviews)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var interviewPackage = this.interviewFactory.GetInteviewEventsPackageOrNull(completedInterview.InterviewId);

                    progress.Report(new SyncProgressInfo
                    {
                        Title = string.Format(InterviewerUIResources.Synchronization_Upload_Title_Format, InterviewerUIResources.Synchronization_Upload_CompletedAssignments_Text),
                        Description = string.Format(InterviewerUIResources.Synchronization_Upload_Description_Format,
                            statistics.SuccessfullyUploadedInterviewsCount, statistics.TotalCompletedInterviewsCount,
                            InterviewerUIResources.Synchronization_Upload_Interviews_Text),
                        Status = SynchronizationStatus.Upload
                    });

                    await this.UploadImagesByCompletedInterviewAsync(completedInterview.InterviewId, progress, cancellationToken);

                    if (interviewPackage != null)
                    {
                        await this.synchronizationService.UploadInterviewAsync(
                            interviewId: completedInterview.InterviewId,
                            completedInterview: interviewPackage,
                            onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                            token: cancellationToken);
                    }
                    else
                    {
                        this.logger.Warn($"Interview event stream is missing. No package was sent to server. Interview ID: {completedInterview.InterviewId}");
                    }

                    this.interviewFactory.RemoveInterview(completedInterview.InterviewId);

                    statistics.SuccessfullyUploadedInterviewsCount++;
                }
                catch (Exception syncException)
                {
                    statistics.FailedToUploadInterviwesCount++;
                    await this.TrySendUnexpectedExceptionToServerAsync(syncException, cancellationToken);
                    this.logger.Error($"Failed to sync interview {completedInterview.Id}. Interviewer login {this.principal.CurrentUserIdentity.Name}", syncException);
                }
            }
        }

        private async Task UploadImagesByCompletedInterviewAsync(Guid interviewId, IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            var imageViews = this.interviewMultimediaViewStorage.Where(image => image.InterviewId == interviewId);

            foreach (var imageView in imageViews)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileView = this.interviewFileViewStorage.GetById(imageView.FileId);
                await this.synchronizationService.UploadInterviewImageAsync(
                    interviewId: imageView.InterviewId,
                    fileName: imageView.FileName,
                    fileData: fileView.File,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: cancellationToken);
                this.interviewMultimediaViewStorage.Remove(imageView.Id);
                this.interviewFileViewStorage.Remove(fileView.Id);
            }
        }

        private void UpdatePasswordOfInterviewer(RestCredentials credentials)
        {
            var localInterviewer = this.interviewersPlainStorage.FirstOrDefault();
            localInterviewer.PasswordHash = this.passwordHasher.Hash(credentials.Password);
            localInterviewer.Token = credentials.Token;

            this.interviewersPlainStorage.Store(localInterviewer);
            this.principal.SignIn(localInterviewer.Name, credentials.Password, true);
        }

        private SyncStatisticsApiView ToSyncStatisticsApiView(SychronizationStatistics statistics, Stopwatch stopwatch)
        {
            var httpStats = this.httpStatistician.GetStats();

            return new SyncStatisticsApiView
            {
                DownloadedInterviewsCount = statistics.NewInterviewsCount,
                UploadedInterviewsCount = statistics.SuccessfullyUploadedInterviewsCount,
                DownloadedQuestionnairesCount = statistics.SuccessfullyDownloadedQuestionnairesCount,
                RejectedInterviewsOnDeviceCount = this.interviewViewRepository.Count(inteview => inteview.Status == InterviewStatus.RejectedBySupervisor),
                NewInterviewsOnDeviceCount = this.interviewViewRepository.Count(inteview => inteview.Status == InterviewStatus.InterviewerAssigned && !inteview.CanBeDeleted),
                TotalDownloadedBytes = httpStats.DownloadedBytes,
                TotalUploadedBytes = httpStats.UploadedBytes,
                TotalConnectionSpeed = httpStats.Speed,
                TotalSyncDuration = stopwatch.Elapsed
            };
        }

        private DeviceInfoApiView ToDeviceInfoApiView(DeviceInfo info) => new DeviceInfoApiView
        {
            DeviceId = info.DeviceId,
            DeviceModel = info.DeviceModel,
            DeviceType = info.DeviceType,
            DeviceDate = info.DeviceDate,
            DeviceLanguage = info.DeviceLanguage,
            DeviceLocation = info.DeviceLocation != null ? ToLocationAddressApiView(info.DeviceLocation) : null,
            DeviceManufacturer = info.DeviceManufacturer,
            DeviceBuildNumber = info.DeviceBuildNumber,
            DeviceSerialNumber = info.DeviceSerialNumber,

            AndroidVersion = info.AndroidVersion,
            AndroidSdkVersion = info.AndroidSdkVersion,
            AndroidSdkVersionName = info.AndroidSdkVersionName,

            AppVersion = info.AppVersion,
            AppBuildVersion = info.AppBuildVersion,
            AppOrientation = info.AppOrientation,
            LastAppUpdatedDate = info.LastAppUpdatedDate,

            BatteryChargePercent = info.BatteryChargePercent,
            BatteryPowerSource = info.BatteryPowerSource,
            IsPowerInSaveMode = info.IsPowerInSaveMode,

            MobileOperator = info.MobileOperator,
            MobileSignalStrength = info.MobileSignalStrength,
            NetworkType = info.NetworkType,
            NetworkSubType = info.NetworkSubType,

            NumberOfStartedInterviews = this.interviewViewRepository.Count(
                interview => interview.StartedDateTime != null && interview.CompletedDateTime == null),

            DBSizeInfo = info.DBSizeInfo,
            StorageInfo = ToStorageInfoApiView(info.StorageInfo),
            RAMInfo = ToRAMInfoApiView(info.RAMInfo)
        };

        private LocationAddressApiView ToLocationAddressApiView(LocationAddress locationAddress)
            => new LocationAddressApiView
            {
                Longitude = locationAddress.Longitude,
                Latitude = locationAddress.Latitude
            };

        private RAMInfoApiView ToRAMInfoApiView(RAMInfo ramInfo) => new RAMInfoApiView
        {
            Free = ramInfo.Free,
            Total = ramInfo.Total
        };

        private StorageInfoApiView ToStorageInfoApiView(StorageInfo storageInfo) => new StorageInfoApiView
        {
            Free = storageInfo.Free,
            Total = storageInfo.Total
        };

        private UnexpectedExceptionApiView ToUnexpectedExceptionApiView(Exception exception)
            => new UnexpectedExceptionApiView
            {
                Message = exception.Message,
                StackTrace = string.Join(Environment.NewLine,
                    exception.UnwrapAllInnerExceptions().Select(ex => $"{ex.Message} {ex.StackTrace}"))
            };
    }
}