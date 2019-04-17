using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.AllowedAddresses;
using WB.Core.Infrastructure.Versions;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models.ControlPanel;

namespace WB.UI.Designer.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    [Area("Admin")]
    public class ControlPanelController : Controller
    {
        private readonly UserManager<DesignerIdentityUser> users;
        private readonly IConfiguration configuration;
        private readonly IAllowedAddressService allowedAddressService;
        private readonly IQuestionnaireCompilationVersionService questionnaireCompilationVersionService;
        private readonly IProductVersion productVersion;
        private readonly IProductVersionHistory productVersionHistory;

        public ControlPanelController(
            UserManager<DesignerIdentityUser> users,
            IConfiguration configuration, 
            IAllowedAddressService allowedAddressService, 
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService,
            IProductVersion productVersion,
            IProductVersionHistory productVersionHistory)
        {
            this.users = users;
            this.configuration = configuration;
            this.allowedAddressService = allowedAddressService;
            this.questionnaireCompilationVersionService = questionnaireCompilationVersionService;
            this.productVersion = productVersion;
            this.productVersionHistory = productVersionHistory;
        }

        public ActionResult Settings()
            => this.View(this.configuration.AsEnumerable().OrderBy(setting => setting.Key));

        public ActionResult Index() => this.View();

        public ActionResult Versions() => this.View();

        public ActionResult MakeAdmin() => this.View();

        public ActionResult CompilationVersions() 
            => this.View("CompilationVersionsViews/CompilationVersions", this.questionnaireCompilationVersionService.GetCompilationVersions());

        [HttpPost]
        public ActionResult RemoveCompilationVersion(Guid questionnaireId)
        {
            this.questionnaireCompilationVersionService.Remove(questionnaireId);
            return RedirectToAction("CompilationVersions");
        }

        public ActionResult AddCompilationVersion() => this.View("CompilationVersionsViews/AddCompilationVersion", new CompilationVersionModel());

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

        public ActionResult AllowedAddresses() => this.View(this.allowedAddressService.GetAddresses());


        [HttpPost]
        public ActionResult RemoveAllowedAddress(int id)
        {
            this.allowedAddressService.Remove(id);
            return RedirectToAction("AllowedAddresses");
        }

        public ActionResult AddAllowedAddress()
        {
            return this.View(new AllowedAddressModel());
        }

        [HttpPost]
        public ActionResult AddAllowedAddress(AllowedAddressModel model)
        {
            if (ModelState.IsValid)
            {
                // just check if ip address
                var ip = IPAddress.Parse(model.Address);
                this.allowedAddressService.Add(new AllowedAddress
                {
                    Description = model.Description,
                    Address = ip.ToString()
                });
                return RedirectToAction("AllowedAddresses");
            }
            return this.View(model);
        }

        public ActionResult EditAllowedAddress(int id)
        {
            var address = this.allowedAddressService.GetAddressById(id);
            return this.View(new AllowedAddressModel
            {
                Id = address.Id,
                Description = address.Description,
                Address = address.Address
            });
        }

        [HttpPost]
        public ActionResult EditAllowedAddress(AllowedAddressModel model)
        {
            if (ModelState.IsValid)
            {
                // just check if ip address
                var ip = IPAddress.Parse(model.Address);
                this.allowedAddressService.Update(new AllowedAddress
                {
                    Id = model.Id,
                    Description = model.Description,
                    Address = ip.ToString()
                });
                return RedirectToAction("AllowedAddresses");
            }
            return this.View(model);
        }


        [HttpPost]
        public async Task<IActionResult> MakeAdmin(MakeAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = await users.FindByNameAsync(model.Login);
                if (account == null)
                    this.Error($"Account '{model.Login}' does not exists");
                else
                {
                    if (model.IsAdmin)
                    {
                        if (await users.IsInRoleAsync(account, "Administrator"))
                            this.Error($"Account '{model.Login}' has administrator role");
                        else
                        {
                            await users.AddToRoleAsync(account, "Administrator");
                            this.Success($"Administrator role for '{model.Login}' successfully added");   
                        }
                    }
                    else
                    {
                        if (!await users.IsInRoleAsync(account, "Administrator"))
                            this.Error($"Account '{model.Login}' is not in administrator role");
                        else
                        {
                            await users.RemoveFromRoleAsync(account, "Administrator");
                            this.Success($"Administrator role for '{model.Login}' successfully removed");    
                        }
                    }
                }
            }

            return this.View();
        }

        public async Task Exceptions() => await ExceptionalMiddleware.HandleRequestAsync(HttpContext);

        public JsonResult GetVersions() =>
            this.Json(new VersionsInfo(
                this.productVersion.ToString(),
                this.productVersionHistory.GetHistory().ToDictionary(
                    change => change.UpdateTimeUtc,
                    change => change.ProductVersion)));

        public class VersionsInfo
        {
            public VersionsInfo(string product, Dictionary<DateTime, string> history)
            {
                this.Product = product;
                this.History = history;
            }

            public string Product { get; }
            public Dictionary<DateTime, string> History { get; }
        }
    }
}
