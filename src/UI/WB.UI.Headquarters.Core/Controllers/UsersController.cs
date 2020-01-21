using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Models.Users;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services.Impl;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly HqUserStore userRepository;

        public UsersController(IAuthorizedUser authorizedUser, HqUserStore userRepository)
        {
            this.authorizedUser = authorizedUser;
            this.userRepository = userRepository;
        }
        
        [Authorize(Roles = "Administrator, Observer")]
        [ActivePage(MenuItem.Headquarters)]
        [Route("/Headquarters")]
        public ActionResult Headquarters()
        {
            return this.View(new 
            {
                DataUrl = Url.Action("AllHeadquarters", "UsersApi"),
                ImpersonateUrl = authorizedUser.IsObserver
                    ? Url.Action("ObservePerson", "Account")
                    : null,
                EditUrl = authorizedUser.IsAdministrator
                    ? Url.Action("Manage")
                    : null,
                CreateUrl = authorizedUser.IsAdministrator
                    ? Url.Action("Create", new{ id = UserRoles.Headquarter })
                    : null,
                ShowAddUser = authorizedUser.IsAdministrator,
                ShowInstruction = !authorizedUser.IsObserving && !authorizedUser.IsObserver,
                ShowContextMenu = authorizedUser.IsObserver,
            });
        }

        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        [ActivePage(MenuItem.Teams)]
        [Route("/Supervisors")]
        public ActionResult Supervisors()
        {
            return this.View(new
            {
                Api = new
                {
                    DataUrl = Url.Action("AllSupervisors", "UsersApi"),
                    ImpersonateUrl = Url.Action("ObservePerson", "Account"),
                    EditUrl = Url.Action("Manage"),
                    CreateUrl = Url.Action("Create", new {id = UserRoles.Supervisor}),
                    ArchiveUsersUrl = Url.Action("ArchiveUsers", "UsersApi")
                },
                CurrentUser = new
                {
                    IsObserver = authorizedUser.IsObserver,
                    IsObserving = authorizedUser.IsObserving,
                    IsAdministrator = authorizedUser.IsAdministrator
                }
            });
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [AntiForgeryFilter]
        public async Task<ActionResult> Manage(Guid? id)
        {
            var user = await this.userRepository.FindByIdAsync(id ?? this.authorizedUser.Id);
            if (user == null) return NotFound("User not found");

            return View(new
            {
                UserInfo = new
                {
                    UserId = user.Id,
                    Email = user.Email,
                    PersonName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    UserName = user.UserName,
                    Role = user.Roles.FirstOrDefault().Id.ToUserRole().ToUiString(),
                    IsOwnProfile = user.Id == this.authorizedUser.Id,
                    IsLockedByHeadquarters = user.IsLockedByHeadquaters,
                    IsLockedBySupervisor = user.IsLockedBySupervisor
                },
                Api = new
                {
                    UpdatePasswordUrl = Url.Action("UpdatePassword"),
                    UpdateUserUrl = Url.Action("UpdateUser")
                }
            });
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [AntiForgeryFilter]
        public ActionResult Create(string id)
        {
            if (!Enum.TryParse(id, true, out UserRoles role))
                return BadRequest("Unknown user type");

            if (this.authorizedUser.IsHeadquarter && !new[] {UserRoles.Supervisor, UserRoles.Interviewer}.Contains(role))
                return Forbid();

            return View(new
            {
                UserInfo = new {Role = role.ToString()},
                Api = new
                {
                    CreateUserUrl = Url.Action("CreateUser"),
                    ResponsiblesUrl = Url.Action("Supervisors", "UsersTypeahead")
                }
            });
        }

        [ActivePage(MenuItem.UserBatchUpload)]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObserverNotAllowed]
        public ActionResult Upload() => View(new
        {
            Api = new
            {
                UploadUsersUrl = Url.Action("Upload"),
                QuestionnairesUrl = Url.Action("Index", "SurveySetup"),
                ImportUsersTemplateUrl = Url.Action("ImportUsersTemplate", "UsersApi"),
                ImportUsersUrl = Url.Action("ImportUsers", "UsersApi"),
                ImportUsersStatusUrl = Url.Action("ImportStatus", "UsersApi"),
                ImportUsersCompleteStatusUrl = Url.Action("ImportCompleteStatus", "UsersApi"),
                ImportUsersCancelUrl = Url.Action("CancelToImportUsers", "UsersApi"),
                SupervisorCreateUrl = Url.Action("Create", new {id = UserRoles.Supervisor}),
                InterviewerCreateUrl = Url.Action("Create", new {id = UserRoles.Interviewer})
            },
            Config = new
            {
                AllowedUploadFileExtensions = new[] {TextExportFile.Extension, TabExportFile.Extention}
            }
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        [Authorize(Roles = "Administrator, Headquarter")]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserModel model)
        {
            if (!this.ModelState.IsValid) return this.ModelState.ErrorsToJsonResult();

            if (!Enum.TryParse(model.Role, true, out UserRoles role))
                return BadRequest("Unknown user type");

            if (this.authorizedUser.IsHeadquarter && !new[] {UserRoles.Supervisor, UserRoles.Interviewer}.Contains(role))
                return Forbid();

            if (role == UserRoles.Interviewer && !model.SupervisorId.HasValue)
                this.ModelState.AddModelError(nameof(CreateUserModel.SupervisorId), FieldsAndValidations.RequiredSupervisorErrorMessage);

            if(await this.userRepository.FindByNameAsync(model.UserName) != null)
                this.ModelState.AddModelError(nameof(CreateUserModel.UserName), FieldsAndValidations.UserName_Taken);

            if (model.SupervisorId.HasValue)
            {
                var supervisor = await this.userRepository.FindByIdAsync(model.SupervisorId.Value);
                if (supervisor == null || !supervisor.IsInRole(UserRoles.Supervisor) || supervisor.IsArchivedOrLocked)
                    this.ModelState.AddModelError(nameof(CreateUserModel.SupervisorId), HQ.SupervisorNotFound);
            }

            if (this.ModelState.IsValid)
            {
                var user = new HqUser
                {
                    Id = Guid.NewGuid(),
                    IsLockedBySupervisor = model.IsLockedBySupervisor,
                    IsLockedByHeadquaters = model.IsLockedByHeadquarters,
                    FullName = model.PersonName,
                    Email = model.Email,
                    UserName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    Profile = model.SupervisorId.HasValue ? new HqUserProfile {SupervisorId = model.SupervisorId} : null
                };

                var identityResult = await this.userRepository.CreateAsync(user);
                if(!identityResult.Succeeded)
                    this.ModelState.AddModelError(nameof(CreateUserModel.UserName), string.Join(@", ", identityResult.Errors.Select(x => x.Description)));
                else
                {
                    identityResult = await this.userRepository.ChangePasswordAsync(user, model.Password);
                    if (!identityResult.Succeeded)
                        this.ModelState.AddModelError(nameof(CreateUserModel.Password), string.Join(@", ", identityResult.Errors.Select(x => x.Description)));
                    else
                        await this.userRepository.AddToRoleAsync(user, model.Role, CancellationToken.None);
                }
            }
            
            return this.ModelState.ErrorsToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        [Authorize(Roles = "Administrator, Headquarter")]
        public async Task<ActionResult> UpdatePassword([FromBody] ChangePasswordModel model)
        {
            if (!this.ModelState.IsValid) return this.ModelState.ErrorsToJsonResult();

            var currentUser = await this.userRepository.FindByIdAsync(model.UserId);
            if (currentUser == null) return NotFound("User not found");

            if (currentUser.IsArchived)
                return BadRequest(FieldsAndValidations.CannotUpdate_CurrentUserIsArchived);

            if (model.UserId == this.authorizedUser.Id)
            {
                bool isPasswordValid = !string.IsNullOrEmpty(model.OldPassword)
                                       && await this.userRepository.CheckPasswordAsync(currentUser,
                                           model.OldPassword);
                if (!isPasswordValid)
                    this.ModelState.AddModelError(nameof(ChangePasswordModel.OldPassword), FieldsAndValidations.OldPasswordErrorMessage);
            }

            if (this.ModelState.IsValid)
            {
                var updateResult = await this.userRepository.ChangePasswordAsync(currentUser, model.Password);

                if (!updateResult.Succeeded)
                    this.ModelState.AddModelError(nameof(ChangePasswordModel.Password), string.Join(@", ", updateResult.Errors.Select(x => x.Description)));
            }

            return this.ModelState.ErrorsToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        [Authorize(Roles = "Administrator, Headquarter")]
        public async Task<ActionResult> UpdateUser([FromBody] EditUserModel editModel)
        {
            if (!this.ModelState.IsValid) return this.ModelState.ErrorsToJsonResult();

            var currentUser = await this.userRepository.FindByIdAsync(editModel.UserId);
            if (currentUser == null) return NotFound("User not found");

            currentUser.Email = editModel.Email;
            currentUser.FullName = editModel.PersonName;
            currentUser.PhoneNumber = editModel.PhoneNumber;
            currentUser.IsLockedByHeadquaters = editModel.IsLockedByHeadquarters;
            currentUser.IsLockedBySupervisor = editModel.IsLockedBySupervisor;

            var updateResult = await this.userRepository.UpdateAsync(currentUser);

            if (!updateResult.Succeeded)
                this.ModelState.AddModelError(nameof(EditUserModel.Email), string.Join(@", ", updateResult.Errors.Select(x => x.Description)));

            return this.ModelState.ErrorsToJsonResult();
        }

        [Authorize(Roles = "Administrator")]
        [ActivePage(MenuItem.Observers)]
        [Route("/Observers")]
        public ActionResult Observers()
        {
            return this.View(new
            {
                DataUrl = Url.Action("AllObservers", "UsersApi"),
                EditUrl = authorizedUser.IsAdministrator
                    ? Url.Action("Manage")
                    : null,
                CreateUrl = authorizedUser.IsAdministrator
                    ? Url.Action("Create", new{ id = UserRoles.Observer })
                    : null,
                ShowAddUser = authorizedUser.IsAdministrator,
            });
        }  
        
        [Authorize(Roles = "Administrator")]
        [ActivePage(MenuItem.ApiUsers)]
        [Route("/ApiUsers")]
        public ActionResult ApiUsers()
        {
            return this.View(new
            {
                DataUrl = Url.Action("AllApiUsers", "UsersApi"),
                EditUrl = authorizedUser.IsAdministrator
                    ? Url.Action("Manage")
                    : null,
                CreateUrl = authorizedUser.IsAdministrator
                    ? Url.Action("Create", new{ id = UserRoles.ApiUser })
                    : null,
                ShowAddUser = authorizedUser.IsAdministrator,
            });
        }     

        [Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
        [ActivePage(MenuItem.Interviewers)]
        [Route("/Interviewers")]
        public ActionResult Interviewers()
        {
            return this.View(new
            {
                DataUrl = Url.Action("AllInterviewers", "UsersApi"),
                ImpersonateUrl = authorizedUser.IsObserver ? Url.Action("ObservePerson", "Account") : null,
                ArchiveUsersUrl = authorizedUser.IsAdministrator ? Url.Action("ArchiveUsers", "UsersApi") : null,
                SupervisorsUrl = Url.Action("SupervisorsCombobox", "Teams"),
                MoveUserToAnotherTeamUrl = Url.Action("MoveUserToAnotherTeam", "UsersApi"),
                InterviewerProfile = Url.Action("Profile", "Interviewer"),
                EditUrl = authorizedUser.IsAdministrator ? Url.Action("Manage") : null,
                CreateUrl = authorizedUser.IsAdministrator ? Url.Action("Create", new{ id = UserRoles.Interviewer }) : null,
                ShowAddUser = (authorizedUser.IsAdministrator || authorizedUser.IsHeadquarter) && !authorizedUser.IsObserving,
                ShowFirstInstructions = !authorizedUser.IsSupervisor && !authorizedUser.IsObserving,
                ShowSupervisorColumn = authorizedUser.IsAdministrator || authorizedUser.IsHeadquarter,
                CanArchiveUnarchive = authorizedUser.IsAdministrator,
                ShowContextMenu = authorizedUser.IsObserver,
                InterviewerIssues = new[]
                {
                    new ComboboxViewItem() { Key = InterviewerFacet.None.ToString(),                  Value = EnumNames.InterviewerFacet_None   },
                    new ComboboxViewItem() { Key = InterviewerFacet.NeverSynchonized.ToString(),      Value = DevicesInterviewers.NeverSynchronized  },
                    new ComboboxViewItem() { Key = InterviewerFacet.NoAssignmentsReceived.ToString(), Value = DevicesInterviewers.NoAssignments },
                    new ComboboxViewItem() { Key = InterviewerFacet.NeverUploaded.ToString(),         Value = DevicesInterviewers.NeverUploaded },
                    new ComboboxViewItem() { Key = InterviewerFacet.TabletReassigned.ToString(),      Value = DevicesInterviewers.TabletReassigned },
                    new ComboboxViewItem() { Key = InterviewerFacet.OutdatedApp.ToString(),           Value = DevicesInterviewers.OldInterviewerVersion },
                    new ComboboxViewItem() { Key = InterviewerFacet.LowStorage.ToString(),            Value = DevicesInterviewers.LowStorage },
                },
                ArchiveStatuses = new[]
                {
                    new ComboboxViewItem() { Key = "false", Value = Pages.Interviewers_ActiveUsers   },
                    new ComboboxViewItem() { Key = "true",  Value = Pages.Interviewers_ArchivedUsers },
                }
            });
        }
    }
}
