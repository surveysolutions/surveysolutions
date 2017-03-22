using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [ValidateInput(false)]
    public class InterviewerController : TeamController
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository;

        public InterviewerController(ICommandService commandService, 
                              ILogger logger,
                              IAuthorizedUser authorizedUser,
                              HqUserManager userManager,
                              IQueryableReadSideRepositoryReader<InterviewSummary>  interviewRepository)
            : base(commandService, logger, authorizedUser, userManager)
        {
            this.interviewRepository = interviewRepository;
        }


        [Authorize(Roles = "Administrator, Headquarter")]
        public async Task<ActionResult> Create(Guid? supervisorId)
        {
            if (!supervisorId.HasValue)
                return this.View(new InterviewerModel() { IsShowSupervisorSelector = true });

            var supervisor = await this.userManager.FindByIdAsync(supervisorId.Value);

            if (supervisor == null) throw new HttpException(404, string.Empty);

            return this.View(new InterviewerModel() {SupervisorId = supervisorId.Value, SupervisorName = supervisor.UserName});
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter")]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        public async Task<ActionResult> Create(InterviewerModel model)
        {
            if (ModelState.IsValid)
            {
                var creationResult = await this.CreateUserAsync(model, UserRoles.Interviewer, model.SupervisorId);
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

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [ActionName("Profile")]
        public async Task<ActionResult> EnumeratorProfile(Guid id)
        {
            var enumerator = await this.userManager.FindByIdAsync(id);
            if (enumerator == null || !enumerator.IsInRole(UserRoles.Interviewer)) return this.HttpNotFound();

            var supervisor = await this.userManager.FindByIdAsync(enumerator.Profile.SupervisorId.Value);

            if (enumerator == null) throw new HttpException(404, string.Empty);

            return this.View(new EnumeratorProfileModel
            {
                Id = enumerator.Id,
                Email = enumerator.Email,
                LoginName = enumerator.UserName,
                FullName = enumerator.FullName,
                Phone = enumerator.PhoneNumber,
                SupervisorName = supervisor.UserName,
                IsArchived = enumerator.IsArchived,
                Assignments = this.ToAssignmentsModel(id)
            });
        }

        private AssignmentsInfoModel ToAssignmentsModel(Guid enumeratorId)
        {
            var newAssignedCount = this.interviewRepository.Query(interviews => interviews.Count(
                interview => interview.ResponsibleId == enumeratorId && interview.Status == InterviewStatus.InterviewerAssigned));

            var completedInterviewCount = this.interviewRepository.Query(interviews => interviews.Count(
                interview => interview.ResponsibleId == enumeratorId && interview.Status == InterviewStatus.Completed));

            var rejectedInterviewCount = this.interviewRepository.Query(interviews => interviews.Count(
                interview => interview.ResponsibleId == enumeratorId && interview.Status == InterviewStatus.RejectedBySupervisor));

            var approvedByHqCount = this.interviewRepository.Query(interviews => interviews.Count(
                interview => interview.ResponsibleId == enumeratorId && interview.Status == InterviewStatus.ApprovedByHeadquarters));

            return new AssignmentsInfoModel
            {
                NewOnDeviceCount = newAssignedCount,
                RejectedCount = rejectedInterviewCount,
                WaitingForApprovalCount = completedInterviewCount,
                ApprovedByHqCount = approvedByHqCount
            };
        }
        
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public async Task<ActionResult> Edit(Guid id)
        {
            var user = await this.userManager.FindByIdAsync(id);

            if (user == null) throw new HttpException(404, string.Empty);
            if (user.IsArchived) throw new HttpException(403, string.Empty);

            return this.View(new UserEditModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    IsLocked = user.IsLockedByHeadquaters,
                    IsLockedBySupervisor = user.IsLockedBySupervisor,
                    UserName = user.UserName,
                    PersonName = user.FullName,
                    PhoneNumber = user.PhoneNumber
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
                var updateResult = await this.UpdateAccountAsync(model).ConfigureAwait(false);
                if (updateResult.Succeeded)
                {
                    this.Success(string.Format(Pages.InterviewerController_EditSuccess, model.UserName));
                    return this.Back();
                }
                AddErrors(updateResult);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult Back()
        {
            return this.RedirectToAction("Index", "Interviewers");
        }
    }
}