using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WB.Core.BoundedContexts.Designer.Views;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.UI.Designer.Code.ImportExport;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Filters;
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
        private readonly IPlainKeyValueStorage<AssistantSettings> appSettingsStorage;
        private readonly DesignerDbContext dbContext;

        public ControlPanelController(
            UserManager<DesignerIdentityUser> users,
            IConfiguration configuration, 
            IQuestionnaireCompilationVersionService questionnaireCompilationVersionService,
            IProductVersion productVersion,
            IProductVersionHistory productVersionHistory,
            IPlainKeyValueStorage<AssistantSettings> appSettingsStorage,
            DesignerDbContext dbContext)
        {
            this.users = users;
            this.configuration = configuration;
            this.questionnaireCompilationVersionService = questionnaireCompilationVersionService;
            this.productVersion = productVersion;
            this.productVersionHistory = productVersionHistory;
            this.appSettingsStorage = appSettingsStorage;
            this.dbContext = dbContext;
        }

        public ActionResult Settings()
            => this.View(this.configuration.AsEnumerable().OrderBy(setting => setting.Key));

        public ActionResult Index() => this.View();

        public ActionResult Versions() => this.View();

        public ActionResult MakeAdmin() => this.View();
        
        [AntiForgeryFilter]
        public ActionResult AssistantSettings()
        {
            var settings = this.appSettingsStorage.GetById(AppSetting.AssistantSettingsKey);

            return this.View(new AssistantSettingsModel
            {
                IsEnabled = settings?.IsEnabled ?? false,
                IsAvailableToAllUsers = settings?.IsAvailableToAllUsers ?? false

            });
        }
        public ActionResult ThrowException(string message = "Use query argument 'message' to display custom message") =>
            throw new ArgumentException(message);

        public ActionResult HttpStatusCode(int? statusCode, string message = "Use query argument 'statusCode' and 'message' to display custom response") =>
            this.StatusCode(statusCode ?? 404, message);

        public ActionResult DocumentSchema() => this.View();

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
            if (compilationVersion == null)
            {
                return NotFound();
            }

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
        public IActionResult AssistantSettings(AssistantSettingsModel model)
        {
            if (ModelState.IsValid)
            {
                var settings = new AssistantSettings
                {
                    IsEnabled = model.IsEnabled,
                    IsAvailableToAllUsers = model.IsAvailableToAllUsers
                };
                this.appSettingsStorage.Store(settings, AppSetting.AssistantSettingsKey);
                dbContext.SaveChanges();
                this.Success("Assistant settings successfully updated");
            }
            
            return this.RedirectToAction("AssistantSettings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeAdmin(MakeAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Login == null)
                    this.Error($"Account '{model.Login}' does not exists");
                else
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

        private static string? questionnaireDocumentSchema = null;
        
        public IActionResult GetSchema([FromQuery]bool? getFile = null)
        {
            questionnaireDocumentSchema ??= ReadSchema();

            if (getFile == true)
                return new FileStreamResult(new MemoryStream(
                    Encoding.UTF8.GetBytes(questionnaireDocumentSchema)), "application/schema+json")
                {
                    FileDownloadName = "questionnaire.schema.json"
                };
            else
                return this.Json(questionnaireDocumentSchema);
        }

        private string ReadSchema()
        {
            var testType = typeof(QuestionnaireImportService);
            var readResourceFile = $"{testType.Namespace}.QuestionnaireSchema.json";

            using Stream? stream = testType.Assembly.GetManifestResourceStream(readResourceFile);
            if (stream == null)
                throw new ArgumentException("Can't find json schema for questionnaire");
            using StreamReader reader = new StreamReader(stream);
            string schemaText = reader.ReadToEnd();
            return schemaText;
        }
        
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
