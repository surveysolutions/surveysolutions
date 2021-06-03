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
        private readonly IQuestionnaireCompilationVersionService questionnaireCompilationVersionService;
        private readonly IProductVersion productVersion;
        private readonly IProductVersionHistory productVersionHistory;

        public ControlPanelController(
            UserManager<DesignerIdentityUser> users,
            IConfiguration configuration, 
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService,
            IProductVersion productVersion,
            IProductVersionHistory productVersionHistory)
        {
            this.users = users;
            this.configuration = configuration;
            this.questionnaireCompilationVersionService = questionnaireCompilationVersionService;
            this.productVersion = productVersion;
            this.productVersionHistory = productVersionHistory;
        }

        public ActionResult Settings()
            => this.View(this.configuration.AsEnumerable().OrderBy(setting => setting.Key));

        public ActionResult Index() => this.View();

        public ActionResult Versions() => this.View();

        public ActionResult MakeAdmin() => this.View();

        public ActionResult ThrowException(string message = "Use query argument 'message' to display custom message") =>
            throw new ArgumentException(message);

        public ActionResult HttpStatusCode(int? statusCode, string message = "Use query argument 'statusCode' and 'message' to display custom response") =>
            this.StatusCode(statusCode ?? 404, message);


        public ActionResult CompilationVersions() 
            => this.View("CompilationVersionsViews/CompilationVersions", this.questionnaireCompilationVersionService.GetCompilationVersions());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveCompilationVersion(Guid questionnaireId)
        {
            this.questionnaireCompilationVersionService.Remove(questionnaireId);
            return RedirectToAction("CompilationVersions");
        }

        public ActionResult AddCompilationVersion() => this.View("CompilationVersionsViews/AddCompilationVersion", new CompilationVersionModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        
        [HttpPost]
        [ValidateAntiForgeryToken]
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
