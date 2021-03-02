using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Code.Workspaces;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Models.Users;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services.Impl;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize]
    [Workspace(WorkspaceConstants.WorkspaceNames.UsersWorkspaceName)]
    public class UsersController : Controller
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly HqUserManager userManager;
        private readonly IPlainKeyValueStorage<ProfileSettings> profileSettingsStorage;
        private UrlEncoder urlEncoder;
        private IOptions<HeadquartersConfig> options;
        private readonly IPlainStorageAccessor<Workspace> workspaces;

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        [TempData]
        public string[] RecoveryCodes { get; set; }

        public UsersController(IAuthorizedUser authorizedUser, 
            HqUserManager userManager, 
            IPlainKeyValueStorage<ProfileSettings> profileSettingsStorage,
            UrlEncoder urlEncoder,
            IOptions<HeadquartersConfig> options,
            IPlainStorageAccessor<Workspace> workspaces)
        {
            this.authorizedUser = authorizedUser;
            this.userManager = userManager;
            this.profileSettingsStorage = profileSettingsStorage;
            this.urlEncoder = urlEncoder;
            this.options = options;
            this.workspaces = workspaces;
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
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        [AntiForgeryFilter]
        [Route("/Manage/{id?}")]
        [ActivePage(MenuItem.ManageAccount)]
        public async Task<ActionResult> Manage(Guid? id)
        {
            var user = await this.userManager.FindByIdAsync((id ?? this.authorizedUser.Id).FormatGuid());
            if (user == null) return NotFound("User not found");

            if (!HasPermissionsToManageUser(user)) return this.Forbid();

            return View(await GetUserInfo(user));
        }

        [HttpGet]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        [AntiForgeryFilter]
        [ActivePage(MenuItem.ManageAccount)]
        [Route("/ChangePassword/{id?}")]
        public async Task<ActionResult> ChangePassword(Guid? id)
        {
            var user = await this.userManager.FindByIdAsync((id ?? this.authorizedUser.Id).FormatGuid());
            if (user == null) return NotFound("User not found");

            if (!HasPermissionsToManageUser(user)) return this.Forbid();

            return View(await GetUserInfo(user));
        }

        [HttpGet]
        [AuthorizeByRole(UserRoles.Administrator)]
        [ActivePage(MenuItem.ManageAccount)]
        [Route("/Workspaces/{id?}")]
        public async Task<ActionResult> Workspaces(Guid id)
        {
            var user = await this.userManager.FindByIdAsync(id.FormatGuid());
            if (user == null) 
                return NotFound();

            var userInfo = await GetUserInfo(user);
            return View(userInfo);
        }
        
        [HttpGet]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        [AntiForgeryFilter]
        [ActivePage(MenuItem.ManageAccount)]
        [Route("/TwoFactorAuthentication/{id?}")]
        public async Task<ActionResult> TwoFactorAuthentication(Guid? id)
        {
            var user = await this.userManager.FindByIdAsync((id ?? this.authorizedUser.Id).FormatGuid());
            if (user == null) return NotFound("User not found");

            if (!HasPermissionsToManageUser(user)) return this.Forbid();

            return View(await GetUserInfo(user));
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        [HttpGet]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        [AntiForgeryFilter]
        [Route("/ResetAuthenticator/{id?}")]
        [ActivePage(MenuItem.ManageAccount)]
        public async Task<ActionResult> ResetAuthenticator(Guid? id)
        {
            var user = await this.userManager.FindByIdAsync((id ?? this.authorizedUser.Id).FormatGuid());
            if (user == null) return NotFound("User not found");
            if (!HasPermissionsToManageUser(user)) return this.Forbid();
            
            return View(await GetUserInfo(user));
        }

        private async Task<dynamic> GetUserInfo(HqUser user)
        {
            UserRoles userRole = user.Roles.First().Id.ToUserRole();
            return new
            {
                UserInfo = new
                {
                    Is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user),
                    RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user),
                    HasAuthenticator = await this.userManager.GetAuthenticatorKeyAsync(user) != null,

                    UserId = user.Id,
                    Email = user.Email,
                    PersonName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    UserName = user.UserName,
                    Role = userRole.ToString(),
                    IsOwnProfile = user.Id == this.authorizedUser.Id,
                    IsLockedByHeadquarters = user.IsLockedByHeadquaters,
                    IsLockedBySupervisor = user.IsLockedBySupervisor,
                    IsObserving = this.authorizedUser.IsObserving,
                    CanBeLockedAsHeadquarters = authorizedUser.IsAdministrator || authorizedUser.IsHeadquarter,
                    CanChangeWorkspacesList = authorizedUser.IsAdministrator && (userRole == UserRoles.Headquarter || userRole == UserRoles.ApiUser),
                    RecoveryCodes = ""
                },
                Api = new
                {
                    GenerateRecoveryCodesUrl = Url.Action("GenerateRecoveryCodes"),
                    ResetAuthenticatorKeyUrl = Url.Action("ResetAuthenticatorKey"),
                    UpdatePasswordUrl = Url.Action("UpdatePassword"),
                    UpdateUserUrl = Url.Action("UpdateUser"),
                    Disable2faUrl = Url.Action("DisableTwoFactor"),

                    SetupAuthenticatorUrl = Url.Action("SetupAuthenticator", new { id = user.Id }),
                    ShowRecoveryCodesUrl = Url.Action("ShowRecoveryCodes", new { id = user.Id }),
                    TwoFactorAuthenticationUrl = Url.Action("TwoFactorAuthentication", new { id = user.Id }),
                    CheckVerificationCodeUrl = Url.Action("CheckVerificationCode", new { id = user.Id })
                }
            };
        }


        [HttpGet]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        [AntiForgeryFilter]
        [Route("/ResetRecoveryCodes/{id?}")]
        [ActivePage(MenuItem.ManageAccount)]
        public async Task<ActionResult> ResetRecoveryCodes(Guid? id)
        {
            var user = await this.userManager.FindByIdAsync((id ?? this.authorizedUser.Id).FormatGuid());
            if (user == null) return NotFound("User not found");
            if (!HasPermissionsToManageUser(user)) return this.Forbid();

            return View(await GetUserInfo(user));
        }


        [HttpGet]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        [ObservingNotAllowed]
        [AntiForgeryFilter]
        [Route("/ShowRecoveryCodes/{id?}")]
        [ActivePage(MenuItem.ManageAccount)]
        public async Task<ActionResult> ShowRecoveryCodes(Guid? id)
        {
            var user = await this.userManager.FindByIdAsync((id ?? this.authorizedUser.Id).FormatGuid());
            if (user == null) return NotFound("User not found");
            if (!HasPermissionsToManageUser(user)) return this.Forbid();

            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                return RedirectToAction("TwoFactorAuthentication", new { id = id });
            }

            return View(new
            {
                UserInfo = new
                {
                    Is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user),
                    RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user),
                    HasAuthenticator = await this.userManager.GetAuthenticatorKeyAsync(user) != null,

                    UserId = user.Id,
                    Email = user.Email,
                    PersonName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    UserName = user.UserName,
                    Role = user.Roles.FirstOrDefault().Id.ToUserRole().ToString(),
                    IsOwnProfile = user.Id == this.authorizedUser.Id,
                    IsLockedByHeadquarters = user.IsLockedByHeadquaters,
                    IsLockedBySupervisor = user.IsLockedBySupervisor,
                    IsObserving = this.authorizedUser.IsObserving,
                    CanBeLockedAsHeadquarters = authorizedUser.IsAdministrator || authorizedUser.IsHeadquarter,

                    RecoveryCodes = string.Join(" ", RecoveryCodes)

                },
                Api = new
                {
                    GenerateRecoveryCodesUrl = Url.Action("GenerateRecoveryCodes"),
                    ResetAuthenticatorKeyUrl = Url.Action("ResetAuthenticatorKey"),
                    UpdatePasswordUrl = Url.Action("UpdatePassword"),
                    UpdateUserUrl = Url.Action("UpdateUser"),
                    Disable2faUrl = Url.Action("DisableTwoFactor"),

                    SetupAuthenticatorUrl = Url.Action("SetupAuthenticator", new { id = user.Id }),
                    ShowRecoveryCodesUrl = Url.Action("ShowRecoveryCodes", new { id = user.Id }),
                    TwoFactorAuthenticationUrl = Url.Action("TwoFactorAuthentication", new { id = user.Id }),
                    CheckVerificationCodeUrl = Url.Action("CheckVerificationCode", new { id = user.Id })
                }
            });
        }

        [HttpGet]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        [AntiForgeryFilter]
        [Route("/Disable2fa/{id?}")]
        [ActivePage(MenuItem.ManageAccount)]
        public async Task<ActionResult> Disable2fa(Guid? id)
        {
            var user = await this.userManager.FindByIdAsync((id ?? this.authorizedUser.Id).FormatGuid());
            if (user == null) return NotFound("User not found");
            if (!HasPermissionsToManageUser(user)) return this.Forbid();

            if(!user.TwoFactorEnabled)
                return RedirectToAction("TwoFactorAuthentication", new { id = id });

            return View(await GetUserInfo(user));
        }


        [HttpGet]
        [ObservingNotAllowed]
        [AntiForgeryFilter]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        [Route("/SetupAuthenticator/{id?}")]
        [ActivePage(MenuItem.ManageAccount)]
        public async Task<ActionResult> SetupAuthenticator(Guid? id)
        {
            var user = await this.userManager.FindByIdAsync((id ?? this.authorizedUser.Id).FormatGuid());
            if (user == null) return NotFound("User not found");

            if (!HasPermissionsToManageUser(user)) return this.Forbid();

            var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
            }
            
            System.Uri uri = new Uri(options.Value?.BaseUrl ?? "https://mysurvey.solutions");
            var authenticatorUri =
                    string.Format(
                    AuthenticatorUriFormat,
                    urlEncoder.Encode("Survey Solutions"),
                    urlEncoder.Encode($"{user.UserName}@{uri.Host}"),
                    unformattedKey);

            return View(new
            {
                UserInfo = new
                {
                    SharedKey = FormatKey(unformattedKey),
                    AuthenticatorUri = authenticatorUri,

                    Is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user),
                    RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user),
                    HasAuthenticator = await this.userManager.GetAuthenticatorKeyAsync(user) != null,

                    UserId = user.Id,
                    Email = user.Email,
                    PersonName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    UserName = user.UserName,
                    Role = user.Roles.FirstOrDefault().Id.ToUserRole().ToString(),
                    IsOwnProfile = user.Id == this.authorizedUser.Id,
                    IsLockedByHeadquarters = user.IsLockedByHeadquaters,
                    IsLockedBySupervisor = user.IsLockedBySupervisor,
                    IsObserving = this.authorizedUser.IsObserving,
                    CanBeLockedAsHeadquarters = authorizedUser.IsAdministrator || authorizedUser.IsHeadquarter,
                },
                Api = new
                {
                    GenerateRecoveryCodesUrl = Url.Action("GenerateRecoveryCodes"),
                    ResetAuthenticatorKeyUrl = Url.Action("ResetAuthenticatorKey"),
                    UpdatePasswordUrl = Url.Action("UpdatePassword"),
                    UpdateUserUrl = Url.Action("UpdateUser"),
                    Disable2faUrl = Url.Action("DisableTwoFactor"),

                    SetupAuthenticatorUrl = Url.Action("SetupAuthenticator", new { id = user.Id }),
                    ShowRecoveryCodesUrl = Url.Action("ShowRecoveryCodes", new { id = user.Id }),
                    TwoFactorAuthenticationUrl = Url.Action("TwoFactorAuthentication", new { id = user.Id }),
                    CheckVerificationCodeUrl = Url.Action("CheckVerificationCode", new { id = user.Id })
                }
            });
        }


        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObservingNotAllowed]
        [AntiForgeryFilter]
        [Route("/Create")]
        [ActivePage(MenuItem.UsersManagement)]
        public ActionResult Create()
        {
            return View(new
            {
                Api = new
                {
                    CreateUserUrl = Url.Action("CreateUser"),
                    SupervisorWorkspaceUrl = Url.Action("WorkspaceSupervisors", "UsersTypeahead"),
                    WorkspacesUrl = Url.Action("Workspaces", "WorkspaceTypeahead"),
                },
                Roles = GetRolesForCreate(),
            });
        }

        private ComboboxViewItem[] GetRolesForCreate()
        {
            var items = new List<ComboboxViewItem>();
            
            void addUserRole(UserRoles useRole)
                => items.Add(new ComboboxViewItem()
                {
                    Key = useRole.ToString(), 
                    Value = useRole.ToUiString()
                });

            addUserRole(UserRoles.Interviewer);
            addUserRole(UserRoles.Supervisor);

            if (authorizedUser.IsAdministrator)
            {
                addUserRole(UserRoles.Headquarter);
                addUserRole(UserRoles.Observer);
                addUserRole(UserRoles.ApiUser);
            }

            return items.ToArray();
        }

        [ActivePage(MenuItem.UsersManagement)]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObservingNotAllowed]
        [Route("/Upload")]
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
                InterviewerCreateUrl = Url.Action("Create", new {id = UserRoles.Interviewer}),
                WorkspacesUrl = Url.Action("Workspaces", "WorkspaceTypeahead"),
            },
            Config = new
            {
                AllowedUploadFileExtensions = new[] {TextExportFile.Extension, TabExportFile.Extention}
            }
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter)]
        [ActivePage(MenuItem.UsersManagement)]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserModel model)
        {
            if (!this.ModelState.IsValid) return this.ModelState.ErrorsToJsonResult();

            if (!Enum.TryParse(model.Role, true, out UserRoles role))
                return BadRequest("Unknown user type");

            if (string.IsNullOrEmpty(model.Workspace))
                return BadRequest("Unknown user workspace");
            
            if (this.authorizedUser.IsHeadquarter && !new[] {UserRoles.Supervisor, UserRoles.Interviewer}.Contains(role))
                return Forbid();

            if (role == UserRoles.Interviewer && !model.SupervisorId.HasValue)
                this.ModelState.AddModelError(nameof(CreateUserModel.SupervisorId), FieldsAndValidations.RequiredSupervisorErrorMessage);

            if(await this.userManager.FindByNameAsync(model.UserName) != null)
                this.ModelState.AddModelError(nameof(CreateUserModel.UserName), FieldsAndValidations.UserName_Taken);

            var workspace = await workspaces.GetByIdAsync(model.Workspace);
            if (workspace == null)
                this.ModelState.AddModelError(nameof(CreateUserModel.Workspace), FieldsAndValidations.WorkspaceMissing);
            
            if (model.SupervisorId.HasValue)
            {
                var supervisor = await this.userManager.FindByIdAsync(model.SupervisorId.FormatGuid());
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
                };

                user.Workspaces.Add(new WorkspacesUsers(workspace!, user, model.SupervisorId));

                var identityResult = await this.userManager.CreateAsync(user, model.Password);
                if (!identityResult.Succeeded)
                {
                    foreach (var error in identityResult.Errors)
                    {
                        if (error.Code.StartsWith("Password"))
                        {
                            this.ModelState.AddModelError(nameof(CreateUserModel.Password),
                                error.Description);                            
                        }
                        else
                        {
                            this.ModelState.AddModelError(nameof(CreateUserModel.UserName), error.Description);
                        }
                    }
                }
                else
                {
                    await this.userManager.AddToRoleAsync(user, model.Role);
                }
            }
            
            return this.ModelState.ErrorsToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        public async Task<ActionResult> UpdatePassword([FromBody] ChangePasswordModel model)
        {
            if (!this.ModelState.IsValid) return this.ModelState.ErrorsToJsonResult();

            var currentUser = await this.userManager.FindByIdAsync(model.UserId.FormatGuid());
            if (currentUser == null) return NotFound("User not found");

            if (!HasPermissionsToManageUser(currentUser)) return this.Forbid();

            if (currentUser.IsArchived)
                this.ModelState.AddModelError(nameof(ChangePasswordModel.Password), FieldsAndValidations.CannotUpdate_CurrentUserIsArchived);

            if (model.UserId == this.authorizedUser.Id)
            {
                bool isPasswordValid = !string.IsNullOrEmpty(model.OldPassword)
                                       && await this.userManager.CheckPasswordAsync(currentUser,
                                           model.OldPassword);
                if (!isPasswordValid)
                    this.ModelState.AddModelError(nameof(ChangePasswordModel.OldPassword), FieldsAndValidations.OldPasswordErrorMessage);
            }

            if (this.ModelState.IsValid)
            {
                var passwordResetToken = await this.userManager.GeneratePasswordResetTokenAsync(currentUser);
                var updateResult = await this.userManager.ResetPasswordAsync(currentUser, passwordResetToken, model.Password);

                if (!updateResult.Succeeded)
                    this.ModelState.AddModelError(nameof(ChangePasswordModel.Password), string.Join(@", ", updateResult.Errors.Select(x => x.Description)));
            }

            return this.ModelState.ErrorsToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        public async Task<ActionResult> UpdateUser([FromBody] EditUserModel editModel)
        {
            if (!this.ModelState.IsValid) return this.ModelState.ErrorsToJsonResult();

            var currentUser = await this.userManager.FindByIdAsync(editModel.UserId.FormatGuid());
            if (currentUser == null) return NotFound("User not found");

            if (!HasPermissionsToManageUser(currentUser)) return this.Forbid();

            if (currentUser.IsArchived)
            {
                if(currentUser.FullName != editModel.PersonName)
                    this.ModelState.AddModelError(nameof(EditUserModel.PersonName), FieldsAndValidations.CannotUpdate_CurrentUserIsArchived);

                if (currentUser.Email != editModel.Email)
                    this.ModelState.AddModelError(nameof(EditUserModel.Email), FieldsAndValidations.CannotUpdate_CurrentUserIsArchived);

                if (currentUser.PhoneNumber != editModel.PhoneNumber)
                    this.ModelState.AddModelError(nameof(EditUserModel.PhoneNumber), FieldsAndValidations.CannotUpdate_CurrentUserIsArchived);

                if (currentUser.IsLockedByHeadquaters != editModel.IsLockedByHeadquarters)
                    this.ModelState.AddModelError(nameof(EditUserModel.IsLockedByHeadquarters), FieldsAndValidations.CannotUpdate_CurrentUserIsArchived);

                if (currentUser.IsLockedBySupervisor != editModel.IsLockedBySupervisor)
                    this.ModelState.AddModelError(nameof(EditUserModel.IsLockedBySupervisor), FieldsAndValidations.CannotUpdate_CurrentUserIsArchived);
                
                if(this.ModelState.IsValid)
                    this.ModelState.AddModelError(nameof(EditUserModel.PersonName), FieldsAndValidations.CannotUpdate_CurrentUserIsArchived);
            }

            if (!authorizedUser.IsAdministrator && !authorizedUser.IsHeadquarter && currentUser.IsLockedByHeadquaters != editModel.IsLockedByHeadquarters)
                return this.Forbid();

            if (this.ModelState.IsValid)
            {
                currentUser.Email = editModel.Email;
                currentUser.FullName = editModel.PersonName;
                currentUser.PhoneNumber = editModel.PhoneNumber;
                currentUser.IsLockedByHeadquaters = editModel.IsLockedByHeadquarters;
                currentUser.IsLockedBySupervisor = editModel.IsLockedBySupervisor;

                var updateResult = await this.userManager.UpdateAsync(currentUser);

                if (!updateResult.Succeeded)
                    this.ModelState.AddModelError(nameof(EditUserModel.Email),
                        string.Join(@", ", updateResult.Errors.Select(x => x.Description)));
            }

            return this.ModelState.ErrorsToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        public async Task<ActionResult> CheckVerificationCode([FromBody] VerificationCodeModel editModel)
        {
            if (!this.ModelState.IsValid) return this.ModelState.ErrorsToJsonResult();

            var currentUser = await this.userManager.FindByIdAsync(editModel.UserId.FormatGuid());
            if (currentUser == null) return NotFound("User not found");

            if (!HasPermissionsToManageUser(currentUser)) return this.Forbid();

            
            if (this.ModelState.IsValid)
            {
                var verificationCode = editModel?.VerificationCode?.Replace(" ", string.Empty).Replace("-", string.Empty);

                var is2faTokenValid = await userManager.VerifyTwoFactorTokenAsync(
                    currentUser, userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);
                
                if (!is2faTokenValid)
                    this.ModelState.AddModelError(nameof(VerificationCodeModel.VerificationCode),
                        "Invalid code");
                else
                {
                    await userManager.SetTwoFactorEnabledAsync(currentUser, true);

                    if (await userManager.CountRecoveryCodesAsync(currentUser) == 0)
                    {
                        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(currentUser, 10);
                        RecoveryCodes = recoveryCodes.ToArray();
                    }
                }
            }

            return this.ModelState.ErrorsToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        public async Task<ActionResult> ResetAuthenticatorKey([FromBody] TwoFAUser editModel)
        {
            if (!this.ModelState.IsValid) return this.ModelState.ErrorsToJsonResult();

            var currentUser = await this.userManager.FindByIdAsync(editModel.UserId.FormatGuid());
            if (currentUser == null) return NotFound("User not found");

            if (!HasPermissionsToManageUser(currentUser)) return this.Forbid();


            if (this.ModelState.IsValid)
            {
                await userManager.SetTwoFactorEnabledAsync(currentUser, false);
                await userManager.ResetAuthenticatorKeyAsync(currentUser);
            }

            return this.ModelState.ErrorsToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        public async Task<ActionResult> DisableTwoFactor([FromBody] TwoFAUser editModel)
        {
            if (!this.ModelState.IsValid) return this.ModelState.ErrorsToJsonResult();

            var currentUser = await this.userManager.FindByIdAsync(editModel.UserId.FormatGuid());
            if (currentUser == null) return NotFound("User not found");

            if (!HasPermissionsToManageUser(currentUser)) return this.Forbid();


            if (this.ModelState.IsValid)
            {
                var disable2faResult = await userManager.SetTwoFactorEnabledAsync(currentUser, false);
                if (!disable2faResult.Succeeded)
                {
                    this.ModelState.AddModelError(nameof(TwoFAUser.UserId),
                        "Invalid User");
                }
            }

            return this.ModelState.ErrorsToJsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObservingNotAllowed]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor, UserRoles.Interviewer, UserRoles.Observer)]
        public async Task<ActionResult> GenerateRecoveryCodes([FromBody] TwoFAUser editModel)
        {
            if (!this.ModelState.IsValid) return this.ModelState.ErrorsToJsonResult();

            var currentUser = await this.userManager.FindByIdAsync(editModel.UserId.FormatGuid());
            if (currentUser == null) return NotFound("User not found");

            if (!HasPermissionsToManageUser(currentUser)) return this.Forbid();

            if (this.ModelState.IsValid)
            {
                var isTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(currentUser);
                var userId = await userManager.GetUserIdAsync(currentUser);
                if (!isTwoFactorEnabled)
                {
                    this.ModelState.AddModelError(nameof(TwoFAUser.UserId),
                        "Invalid User");
                }
                else
                {
                    var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(currentUser, 10);
                    RecoveryCodes = recoveryCodes.ToArray();
                }
            }

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
            var canAddUser = (authorizedUser.IsAdministrator || authorizedUser.IsHeadquarter) && !authorizedUser.IsObserving;

            return this.View(new
            {
                DataUrl = Url.Action("AllInterviewers", "UsersApi"),
                ImpersonateUrl = authorizedUser.IsObserver ? Url.Action("ObservePerson", "Account") : null,
                ArchiveUsersUrl = authorizedUser.IsAdministrator ? Url.Action("ArchiveUsers", "UsersApi") : null,
                SupervisorsUrl = Url.Action("SupervisorsCombobox", "Teams"),
                MoveUserToAnotherTeamUrl = Url.Action("MoveUserToAnotherTeam", "UsersApi"),
                InterviewerProfile = Url.Action("Profile", "Interviewer"),
                EditUrl = authorizedUser.IsAdministrator ? Url.Action("Manage") : null,
                CreateUrl = canAddUser ? Url.Action("Create", new{ id = UserRoles.Interviewer }) : null,
                ShowFirstInstructions = canAddUser,
                ShowSupervisorColumn = authorizedUser.IsAdministrator || authorizedUser.IsHeadquarter,
                CanArchiveUnarchive = authorizedUser.IsAdministrator,
                CanArchiveMoveToOtherTeam = authorizedUser.IsAdministrator || authorizedUser.IsHeadquarter,
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

        private bool HasPermissionsToManageUser(HqUser user)
        {
            if (this.authorizedUser.IsAdministrator)
                return true;

            if (this.authorizedUser.IsHeadquarter && (user.Id == this.authorizedUser.Id  || user.IsInRole(UserRoles.Supervisor) || user.IsInRole(UserRoles.Interviewer)))
                return true;

            if (this.authorizedUser.IsSupervisor && (user.Id == this.authorizedUser.Id || (user.IsInRole(UserRoles.Interviewer) && user.Profile?.SupervisorId == this.authorizedUser.Id)))
                return true;

            if (this.authorizedUser.IsInterviewer 
                && user.Id == this.authorizedUser.Id
                && (this.profileSettingsStorage.GetById(AppSetting.ProfileSettings)?.AllowInterviewerUpdateProfile ?? false))
                return true;

            if (this.authorizedUser.IsObserver && user.Id == this.authorizedUser.Id)
                return true;

            return false;
        }
    }
}
