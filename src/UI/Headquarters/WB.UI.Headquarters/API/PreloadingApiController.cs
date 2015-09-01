using System;
using System.Linq;
using System.Web.Http;
using Resources;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.Models.UserBatchUpload;

namespace WB.UI.Headquarters.API
{
    public class PreloadingApiController : ApiController
    {
        private readonly IUserPreloadingService userPreloadingService;

        public PreloadingApiController(IUserPreloadingService userPreloadingService)
        {
            this.userPreloadingService = userPreloadingService;
        }

        public UserPreloadingProcess UserPreloadingDetails(string id)
        {
            return userPreloadingService.GetPreloadingProcesseDetails(id);
        }

        [ObserverNotAllowedApi]
        [HttpPost]
        public void DeleteUserBatchUploadProcess(string id)
        {
            this.userPreloadingService.DeletePreloadingProcess(id);
        }

        public UserBatchUploadProcessesView AllUserBatchUploadProcesses(AllUserBatchUploadProcessesRequest request)
        {
            var userPreloadingProcesses = this.userPreloadingService.GetPreloadingProcesses();
            var userPreloadingProcessViews =
                userPreloadingProcesses.Skip((request.PageIndex - 1)*request.PageSize).Take(request.PageSize);

            return new UserBatchUploadProcessesView()
            {
                Items = userPreloadingProcessViews.Select(this.ConvertUserBatchUploadProcessModelToView),
                PageSize = request.PageSize,
                Page = request.PageIndex,
                TotalCount = userPreloadingProcesses.Length
            };
        }

        private UserBatchUploadProcessView ConvertUserBatchUploadProcessModelToView(
            UserPreloadingProcess userBatchUploadProcess)
        {
            return new UserBatchUploadProcessView()
            {
                ProcessId = userBatchUploadProcess.UserPreloadingProcessId,
                FileName = userBatchUploadProcess.FileName,
                FileSize = FileSizeUtils.SizeSuffix(userBatchUploadProcess.FileSize),
                UploadDate =
                    TimeZoneInfo.ConvertTimeFromUtc(userBatchUploadProcess.UploadDate, TimeZoneInfo.Local).ToString("g"),
                LastUpdateDate =
                    TimeZoneInfo.ConvertTimeFromUtc(userBatchUploadProcess.LastUpdateDate, TimeZoneInfo.Local)
                        .ToString("g"),
                Status = UserPrelodingStateToLocalizeString(userBatchUploadProcess.State),
                CanDeleteFile =
                    !new[] {UserPrelodingState.CreatingUsers, UserPrelodingState.Validating}.Contains(
                        userBatchUploadProcess.State),
                State = UserPrelodingStateToUserBatchUploadProcessState(userBatchUploadProcess.State)
            };
        }

        private UserBatchUploadProcessState UserPrelodingStateToUserBatchUploadProcessState(UserPrelodingState state)
        {
            switch (state)
            {
                case UserPrelodingState.ReadyForUserCreation:
                case UserPrelodingState.CreatingUsers:
                case UserPrelodingState.Finished:
                case UserPrelodingState.FinishedWithError:
                    return UserBatchUploadProcessState.Started;

                case UserPrelodingState.ReadyForValidation:
                case UserPrelodingState.Validating:
                case UserPrelodingState.Validated:
                    return UserBatchUploadProcessState.InProgress;

                default:
                    return UserBatchUploadProcessState.Finished;
            }
        }

        private static string UserPrelodingStateToLocalizeString(UserPrelodingState state)
        {
            switch (state)
            {
                case UserPrelodingState.CreatingUsers:
                    return BatchUpload.CreatingUsers;
                case UserPrelodingState.Finished:
                    return BatchUpload.Finished;
                case UserPrelodingState.FinishedWithError:
                    return BatchUpload.FinishedWithError;
                case UserPrelodingState.ReadyForUserCreation:
                    return BatchUpload.ReadyForUserCreation;
                case UserPrelodingState.ReadyForValidation:
                    return BatchUpload.ReadyForValidation;
                case UserPrelodingState.Uploaded:
                    return BatchUpload.Uploaded;
                case UserPrelodingState.Validated:
                    return BatchUpload.Validated;
                case UserPrelodingState.Validating:
                    return BatchUpload.Validating;
                case UserPrelodingState.ValidationFinishedWithError:
                    return BatchUpload.ValidationFinishedWithError;
            }
            return null;
        }
    }
}