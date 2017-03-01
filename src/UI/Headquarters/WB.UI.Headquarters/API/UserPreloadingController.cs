using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Resources;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.UserPreloading;

namespace WB.UI.Headquarters.API
{
    public class UserPreloadingController : ApiController
    {
        private readonly IUserPreloadingService userPreloadingService;

        public UserPreloadingController(IUserPreloadingService userPreloadingService)
        {
            this.userPreloadingService = userPreloadingService;
        }

        public UserPreloadingProcess UserPreloadingDetails(string id)
        {
            return userPreloadingService.GetPreloadingProcesseDetails(id);
        }

        [ObserverNotAllowedApi]
        [HttpPost]
        public HttpResponseMessage DeleteUserPreloadingProcess(string id)
        {
            try
            {
                this.userPreloadingService.DeletePreloadingProcess(id);
            }
            catch (Exception e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }
            return Request.CreateResponse(true);
        }

        public UserPreloadingProcessesView AllUserPreloadingProcesses(AllUserPreloadingProcessesRequest request)
        {
            var userPreloadingProcesses = this.userPreloadingService.GetPreloadingProcesses();
            var userPreloadingProcessViews =
                userPreloadingProcesses.Skip((request.PageIndex - 1)*request.PageSize).Take(request.PageSize);

            return new UserPreloadingProcessesView()
            {
                Items = userPreloadingProcessViews.Select(this.ConvertUserPreloadingProcessModelToView),
                PageSize = request.PageSize,
                Page = request.PageIndex,
                TotalCount = userPreloadingProcesses.Length
            };
        }

        private UserPreloadingProcessView ConvertUserPreloadingProcessModelToView(
            UserPreloadingProcess userPreloadingProcess)
        {
            return new UserPreloadingProcessView()
            {
                ProcessId = userPreloadingProcess.UserPreloadingProcessId,
                FileName = userPreloadingProcess.FileName,
                FileSize = FileSizeUtils.SizeSuffix(userPreloadingProcess.FileSize),
                UploadDate =
                    TimeZoneInfo.ConvertTimeFromUtc(userPreloadingProcess.UploadDate, TimeZoneInfo.Local).ToString("g"),
                LastUpdateDate =
                    TimeZoneInfo.ConvertTimeFromUtc(userPreloadingProcess.LastUpdateDate, TimeZoneInfo.Local)
                        .ToString("g"),
                Status = UserPrelodingStateToLocalizeString(userPreloadingProcess.State),
                CanDeleteFile =
                    !new[] {UserPrelodingState.CreatingUsers, UserPrelodingState.Validating}.Contains(
                        userPreloadingProcess.State),
                State = this.UserPrelodingStateToUserPreloadingProcessState(userPreloadingProcess.State)
            };
        }

        private UserPreloadingProcessState UserPrelodingStateToUserPreloadingProcessState(UserPrelodingState state)
        {
            switch (state)
            {
                case UserPrelodingState.ReadyForUserCreation:
                case UserPrelodingState.CreatingUsers:
                case UserPrelodingState.Finished:
                case UserPrelodingState.FinishedWithError:
                    return UserPreloadingProcessState.Started;

                case UserPrelodingState.ReadyForValidation:
                case UserPrelodingState.Validating:
                case UserPrelodingState.Validated:
                    return UserPreloadingProcessState.InProgress;

                default:
                    return UserPreloadingProcessState.Finished;
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