using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using Main.Core.Entities.SubEntities;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Documents;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [ValidateInput(false)]
    public class InterviewerController : TeamController
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository;
        private readonly IDeviceSyncInfoRepository deviceSyncInfoRepository;
        private readonly IInterviewerVersionReader interviewerVersionReader;
        private readonly IReadSideRepositoryWriter<TabletDocument> tabletDocumentReader;

        public InterviewerController(ICommandService commandService,
                              ILogger logger,
                              IAuthorizedUser authorizedUser,
                              HqUserManager userManager,
                              IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository,
                              IDeviceSyncInfoRepository deviceSyncInfoRepository, IInterviewerVersionReader interviewerVersionReader,
                              IReadSideRepositoryWriter<TabletDocument> tabletDocumentReader)
            : base(commandService, logger, authorizedUser, userManager)
        {
            this.interviewRepository = interviewRepository;
            this.deviceSyncInfoRepository = deviceSyncInfoRepository;
            this.interviewerVersionReader = interviewerVersionReader;
            this.tabletDocumentReader = tabletDocumentReader;
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public async Task<ActionResult> Create(Guid? supervisorId)
        {
            if (!supervisorId.HasValue)
                return this.View(new InterviewerModel() { IsShowSupervisorSelector = true });

            var supervisor = await this.userManager.FindByIdAsync(supervisorId.Value);

            if (supervisor == null) throw new HttpException(404, string.Empty);

            return this.View(new InterviewerModel
            {
                SupervisorId = supervisorId.Value,
                SupervisorName = supervisor.UserName,
                IsLockedDisabled = !(this.authorizedUser.IsHeadquarter || authorizedUser.IsAdministrator)
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> Create(InterviewerModel model)
        {
            if (ModelState.IsValid)
            {
                var creationResult = await this.CreateUserAsync(model, UserRoles.Interviewer, model.SupervisorId, isLockedBySupervisor: model.IsLockedBySupervisor);
                if (creationResult.Succeeded)
                {
                    this.Success(Pages.InterviewerController_InterviewerCreationSuccess);
                    return this.Back();
                }
                AddErrors(creationResult);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor, Interviewer")]
        [ActionName("Profile")]
        public async Task<ActionResult> InterviewerProfile(Guid? id)
        {
            var userId = id ?? this.authorizedUser.Id;
            var interviewer = await this.userManager.FindByIdAsync(userId);
            if (interviewer == null || !interviewer.IsInRole(UserRoles.Interviewer)) return this.HttpNotFound();

            var supervisor = await this.userManager.FindByIdAsync(interviewer.Profile.SupervisorId.Value);

            if (interviewer == null) throw new HttpException(404, string.Empty);

            var completedInterviewCount = this.interviewRepository.Query(interviews => interviews.Count(
                interview => interview.ResponsibleId == userId && interview.Status == InterviewStatus.Completed));

            var approvedByHqCount = this.interviewRepository.Query(interviews => interviews.Count(
                interview => interview.ResponsibleId == userId && interview.Status == InterviewStatus.ApprovedByHeadquarters));

            var lastSuccessDeviceInfo = this.deviceSyncInfoRepository.GetLastSuccessByInterviewerId(userId);
            var hasUpdateForInterviewerApp = false;
            DateTime? deviceAssignmentDate = null;

            if (lastSuccessDeviceInfo != null)
            {
                int? interviewerApkVersion = interviewerVersionReader.Version;
                hasUpdateForInterviewerApp = interviewerApkVersion.HasValue && interviewerApkVersion.Value > lastSuccessDeviceInfo.AppBuildVersion;

                var magicDeviceId = lastSuccessDeviceInfo.DeviceId.ToGuid().FormatGuid();
                var tabletDocument = this.tabletDocumentReader.GetById(magicDeviceId);
                deviceAssignmentDate = tabletDocument.RegistrationDate;
            }

            var interviewerProfileModel = new InterviewerProfileModel
            {
                Id = interviewer.Id,
                Email = interviewer.Email,
                LoginName = interviewer.UserName,
                IsArchived = interviewer.IsArchived,
                FullName = interviewer.FullName,
                Phone = interviewer.PhoneNumber,
                SupervisorName = supervisor.UserName,
                HasUpdateForInterviewerApp = hasUpdateForInterviewerApp,
                WaitingInterviewsForApprovalCount = completedInterviewCount,
                ApprovedInterviewsByHqCount = approvedByHqCount,
                TotalSuccessSynchronizationCount = this.deviceSyncInfoRepository.GetSuccessSynchronizationsCount(userId),
                TotalFailedSynchronizationCount = this.deviceSyncInfoRepository.GetFailedSynchronizationsCount(userId),
                LastSuccessDeviceInfo = lastSuccessDeviceInfo,
                LastSyncronizationDate = this.deviceSyncInfoRepository.GetLastSyncronizationDate(userId),
                LastFailedDeviceInfo = this.deviceSyncInfoRepository.GetLastFailedByInterviewerId(userId),
                AverageSyncSpeedBytesPerSecond = this.deviceSyncInfoRepository.GetAverageSynchronizationSpeedInBytesPerSeconds(userId),
                SynchronizationActivity = this.deviceSyncInfoRepository.GetSynchronizationActivity(userId, interviewer.Profile.DeviceId),
                DeviceAssignmentDate = deviceAssignmentDate
            };
            return this.View(interviewerProfileModel);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public async Task<ActionResult> Edit(Guid id)
        {
            var user = await this.userManager.FindByIdAsync(id);

            if (user == null) throw new HttpException(404, string.Empty);
            if (user.IsArchived) throw new HttpException(403, string.Empty);
            if (!user.IsInRole(UserRoles.Interviewer)) throw new HttpException(403, HQ.NoPermission);

            return this.View(new UserEditModel
            {
                Id = user.Id,
                Email = user.Email,
                IsLocked = user.IsLockedByHeadquaters,
                IsLockedBySupervisor = user.IsLockedBySupervisor,
                UserName = user.UserName,
                PersonName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                CancelAction = "Profile",
                CancelArg = new { id },
                IsLockedDisabled = !(this.authorizedUser.IsHeadquarter || authorizedUser.IsAdministrator)
            });
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> Edit(UserEditModel model)
        {
            if (ModelState.IsValid)
            {
                var updateResult = await this.UpdateAccountAsync(model);
                if (updateResult.Succeeded)
                {
                    this.Success(string.Format(Pages.InterviewerController_EditSuccess, model.UserName));
                    return this.RedirectToAction(nameof(this.Profile), new { id = model.Id });
                }
                AddErrors(updateResult);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> UnArchive(Guid id)
        {
            var interviewer = await this.userManager.FindByIdAsync(id);
            if (interviewer == null)
                throw new HttpException(404, string.Empty);

            if (!interviewer.IsInRole(UserRoles.Interviewer))
                throw new HttpException(403, string.Empty);

            await this.userManager.UnarchiveUsersAsync(new[] { id });

            return RedirectToAction("Profile", new { id = id });
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult Back()
        {
            return this.RedirectToAction("Index", "Interviewers");
        }

        public override ActionResult Cancel(Guid? id)
        {
            if (id.HasValue)
            {
                return RedirectToAction("Profile", new {id});
            }
            else
            {
                return RedirectToAction("Back");
            }
        }
    }
}