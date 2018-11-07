using System;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Resources;
using WB.Core.BoundedContexts.Headquarters.InterviewerProfiles;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [ValidateInput(false)]
    public class InterviewerController : TeamController
    {
        private readonly IInterviewerProfileFactory interviewerProfileFactory;

        public InterviewerController(ICommandService commandService,
                              ILogger logger,
                              IAuthorizedUser authorizedUser,
                              HqUserManager userManager,
                              IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository,
                              IDeviceSyncInfoRepository deviceSyncInfoRepository, IInterviewerVersionReader interviewerVersionReader, 
                              IInterviewerProfileFactory interviewerProfileFactory)
            : base(commandService, logger, authorizedUser, userManager)
        {
            this.interviewerProfileFactory = interviewerProfileFactory;
        }

        [AuthorizeOr403(Roles = "Administrator, Headquarter")]
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
        [AuthorizeOr403(Roles = "Administrator, Headquarter")]
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

        [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor, Interviewer")]
        [ActionName("Profile")]
        public async Task<ActionResult> InterviewerProfile(Guid? id)
        {
            var userId = id ?? this.authorizedUser.Id;
            var interviewer = await this.userManager.FindByIdAsync(userId);
            if (interviewer == null || !interviewer.IsInRole(UserRoles.Interviewer)) return this.HttpNotFound();

            InterviewerProfileModel interviewerProfileModel = await interviewerProfileFactory.GetInterviewerProfileAsync(userId);

            if (interviewerProfileModel == null) throw new HttpException(404, string.Empty);

            return this.View(interviewerProfileModel);
        }

        [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor, Interviewer")]
        [CamelCase]
        public async Task<ActionResult> InterviewerPoints(Guid? id)
        {
            var userId = id ?? this.authorizedUser.Id;
            var interviewer = await this.userManager.FindByIdAsync(userId);
            if (interviewer == null || !interviewer.IsInRole(UserRoles.Interviewer)) return this.HttpNotFound();

            var points = interviewerProfileFactory.GetInterviewerCheckinPoints(userId).ToList();

            if (points.Count == 0) return this.HttpNotFound("No points");

            return this.Json(points, JsonRequestBehavior.AllowGet);
        }

        
        [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor, Interviewer")]
        [ActionName("TrafficUsage")]
        [CamelCase]
        public async Task<ActionResult> InterviewerTrafficUsage(Guid? id)
        {
            var userId = id ?? this.authorizedUser.Id;
            var interviewer = await this.userManager.FindByIdAsync(userId);
            if (interviewer == null || !interviewer.IsInRole(UserRoles.Interviewer)) return this.HttpNotFound();

            InterviewerTrafficUsage trafficUsage = await interviewerProfileFactory.GetInterviewerTrafficUsageAsync(userId);

            if (trafficUsage == null) return this.HttpNotFound();

            return new ContentResult
            {
                ContentType = "application/json",
                Content = JsonConvert.SerializeObject(trafficUsage, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                ContentEncoding = Encoding.UTF8
            };
        }

        [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor")]
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

        [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor")]
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

        [AuthorizeOr403(Roles = "Administrator")]
        [HttpPost]
        public async Task<ActionResult> UnArchive(Guid id)
        {
            var interviewer = await this.userManager.FindByIdAsync(id);
            if (interviewer == null)
                throw new HttpException(404, string.Empty);

            if (!interviewer.IsInRole(UserRoles.Interviewer))
                throw new HttpException(403, string.Empty);

            var unarchiveResult = await this.userManager.UnarchiveUsersAsync(new[] { id });
            var identityResult = unarchiveResult.First();
            if (!identityResult.Succeeded)
            {
                Error(identityResult.Errors.FirstOrDefault());
            }

            return RedirectToAction("Profile", new { id = id });
        }

        [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor")]
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
