using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.AllowedAddresses;
using WB.UI.Designer.Models;
using WB.UI.Designer.Models.ControlPanel;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Designer.Controllers
{
    [ControlPanelAccess]
    public class ControlPanelController : BaseController
    {
        readonly ISettingsProvider settingsProvider;
        private readonly IAllowedAddressService allowedAddressService;
        private readonly IQuestionnaireCompilationVersionService questionnaireCompilationVersionService;

        public ControlPanelController(
            IMembershipUserService userHelper, 
            ISettingsProvider settingsProvider, 
            IAllowedAddressService allowedAddressService, 
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService)
            : base(userHelper)
        {
            this.settingsProvider = settingsProvider;
            this.allowedAddressService = allowedAddressService;
            this.questionnaireCompilationVersionService = questionnaireCompilationVersionService;
        }

        public ActionResult Settings() => this.View(this.settingsProvider.GetSettings().OrderBy(setting => setting.Name));

        public ActionResult Index() => this.View();

        public ActionResult NConfig() => this.View();

        public ActionResult Versions() => this.View();

        public ActionResult MakeAdmin() => this.View();

        public ActionResult CompilationVersions() => this.View("CompilationVersionsViews/CompilationVersions", this.questionnaireCompilationVersionService.GetCompilationVersions());

        [HttpPost]
        public ActionResult RemoveCompilationVersion(Guid questionnaireId)
        {
            this.questionnaireCompilationVersionService.Remove(questionnaireId);
            return RedirectToAction("CompilationVersions");
        }

        public ActionResult AddCompilationVersion()
        {
            return this.View("CompilationVersionsViews/AddCompilationVersion", new CompilationVersionModel());
        }

        [HttpPost]
        public ActionResult AddCompilationVersion(CompilationVersionModel model)
        {
            if (ModelState.IsValid)
            {
                this.questionnaireCompilationVersionService.Add(new QuestionnaireCompilationVersion
                {
                    QuestionnaireId = model.QuestionnaireId,
                    Description = model.Description,
                    Version= model.Version
                });
                return RedirectToAction("CompilationVersions");
            }
            return this.View("CompilationVersionsViews/AddCompilationVersion", model);
        }

        public ActionResult EditCompilationVersion(Guid id)
        {
            var compilationVersion = this.questionnaireCompilationVersionService.GetById(id);
            return this.View("CompilationVersionsViews/EditCompilationVersion", new CompilationVersionModel
            {
                QuestionnaireId = compilationVersion.QuestionnaireId,
                Description = compilationVersion.Description,
                Version = compilationVersion.Version
            });
        }

        [HttpPost]
        public ActionResult EditCompilationVersion(CompilationVersionModel model)
        {
            if (ModelState.IsValid)
            {
                this.questionnaireCompilationVersionService.Update(new QuestionnaireCompilationVersion
                {
                    QuestionnaireId = model.QuestionnaireId,
                    Description = model.Description,
                    Version = model.Version
                });
                return RedirectToAction("CompilationVersions");
            }
            return this.View("CompilationVersionsViews/EditCompilationVersion", model);
        }

        public ActionResult AllowesAddresses() => this.View(this.allowedAddressService.GetAddresses());


        [HttpPost]
        public ActionResult RemoveAllowesAddress(int id)
        {
            this.allowedAddressService.Remove(id);
            return RedirectToAction("AllowesAddresses");
        }

        public ActionResult AddAllowesAddress()
        {
            return this.View(new AllowedAddressModel());
        }

        [HttpPost]
        public ActionResult AddAllowesAddress(AllowedAddressModel model)
        {
            if (ModelState.IsValid)
            {
                this.allowedAddressService.Add(new AllowedAddress
                {
                    Description = model.Description,
                    Address = IPAddress.Parse(model.Address)
                });
                return RedirectToAction("AllowesAddresses");
            }
            return this.View(model);
        }

        public ActionResult EditAllowesAddress(int id)
        {
            var address = this.allowedAddressService.GetAddressById(id);
            return this.View(new AllowedAddressModel
            {
                Id = address.Id,
                Description = address.Description,
                Address = address.Address.ToString()
            });
        }

        [HttpPost]
        public ActionResult EditAllowesAddress(AllowedAddressModel model)
        {
            if (ModelState.IsValid)
            {
                this.allowedAddressService.Update(new AllowedAddress
                {
                    Id = model.Id,
                    Description = model.Description,
                    Address = IPAddress.Parse(model.Address)
                });
                return RedirectToAction("AllowesAddresses");
            }
            return this.View(model);
        }


        [HttpPost]
        public ActionResult MakeAdmin(MakeAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = Membership.GetUser(model.Login);
                if (account == null)
                {
                    this.Error(string.Format("Account '{0}' does not exists", model.Login));
                }
                else
                {
                    if (model.IsAdmin)
                    {
                        if (Roles.IsUserInRole(model.Login, UserHelper.ADMINROLENAME))
                        {
                            this.Error(string.Format("Account '{0}' has administrator role", model.Login));
                        }
                        else
                        {
                            Roles.AddUserToRole(account.ProviderUserKey.ToString(), UserHelper.ADMINROLENAME);
                            this.Success(string.Format("Administrator role for '{0}' successfully added", model.Login));   
                        }
                    }
                    else
                    {
                        if (!Roles.IsUserInRole(model.Login, UserHelper.ADMINROLENAME))
                        {
                            this.Error(string.Format("Account '{0}' is not in administrator role", model.Login));
                        }
                        else
                        {
                            Roles.RemoveUserFromRole(model.Login, UserHelper.ADMINROLENAME);
                            this.Success(string.Format("Administrator role for '{0}' successfully removed", model.Login));    
                        }
                    }
                }
            }

            return this.View();
        }

        public Task Exceptions() => ExceptionalModule.HandleRequestAsync(System.Web.HttpContext.Current);
    }
}
