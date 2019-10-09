using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Humanizer;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;

using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Models.Admin;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Headquarters.Controllers
{
    [ControlPanelAccess]
    public class ControlPanelController : BaseController
    {
        private readonly HqUserManager userManager;
        private readonly IServiceLocator serviceLocator;
        private readonly ISettingsProvider settingsProvider;
        private readonly IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings;
        private readonly IAssignmentsService assignmentsService;
        private readonly IAndroidPackageReader androidPackageReader;
        private readonly IInterviewPackagesService interviewPackagesService;
        private readonly IUserViewFactory userViewFactory;
        private readonly IJsonAllTypesSerializer serializer;
        private readonly IClientApkProvider clientApkProvider;
        private readonly IAuthorizedUser currentUser;

        public ControlPanelController(
            IServiceLocator serviceLocator,
            ICommandService commandService,
            HqUserManager userManager,
            ILogger logger,
            ISettingsProvider settingsProvider,
            IAndroidPackageReader androidPackageReader,
            IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings,
            IAssignmentsService assignmentsService, 
            IInterviewPackagesService interviewPackagesService, 
            IUserViewFactory userViewFactory, 
            IJsonAllTypesSerializer serializer,
            IClientApkProvider clientApkProvider,
            IAuthorizedUser currentUser)
             : base(commandService: commandService, logger: logger)
        {
            this.userManager = userManager;
            this.androidPackageReader = androidPackageReader;
            this.serviceLocator = serviceLocator;
            this.settingsProvider = settingsProvider;
            this.exportServiceSettings = exportServiceSettings;
            this.assignmentsService = assignmentsService;
            this.interviewPackagesService = interviewPackagesService;
            this.userViewFactory = userViewFactory;
            this.serializer = serializer;
            this.clientApkProvider = clientApkProvider;
            this.currentUser = currentUser;
        }

        public ActionResult CreateHeadquarters()
        {
            return this.View(new UserModel());
        }

        public ActionResult CreateAdmin()
        {
            return this.View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateHeadquarters(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var creationResult = await this.userManager.CreateUserAsync(
                            new HqUser
                            {
                                Id = Guid.NewGuid(),
                                UserName = model.UserName,
                                Email = model.Email,
                                FullName = model.PersonName,
                                PhoneNumber = model.PhoneNumber
                            }, model.Password, UserRoles.Headquarter);

                if (creationResult.Succeeded)
                {
                    this.Success(@"Headquarters successfully created");
                    return this.RedirectToAction("LogOn", "Account");
                }
                AddErrors(creationResult.Errors);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAdmin(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var creationResult = await this.userManager.CreateUserAsync(
                    new HqUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = model.UserName,
                        Email = model.Email,
                        FullName = model.PersonName,
                        PhoneNumber = model.PhoneNumber
                    }, model.Password, UserRoles.Administrator);

                if (creationResult.Succeeded)
                {
                    this.Success(@"Administrator successfully created");
                    return this.RedirectToAction("LogOn", "Account");
                }
                AddErrors(creationResult.Errors);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult ResetPrivilegedUserPassword()
        {
            return this.View(new UserEditModel());
        }

        public async Task<ActionResult> ExportService()
        {
            try
            {
                var exportServiceApi = serviceLocator.GetInstance<IExportServiceApi>();
                var health = await exportServiceApi.Health();

                this.ViewData["version"] = await exportServiceApi.Version();
                this.ViewData["health"] = await health.Content.ReadAsStringAsync();
                this.ViewData["uri"] = health.RequestMessage.RequestUri.ToString();
            }
            catch (Exception e)
            {
                this.ViewData["health"] = e.ToString();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPrivilegedUserPassword(UserEditModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await this.userManager.FindByNameAsync(model.UserName);
                var updateResult = await this.userManager.ChangePasswordAsync(user, model.Password);

                if (updateResult.Succeeded)
                {
                    this.Success($"Password for user '{user.UserName}' successfully changed");
                }
                else
                {
                    AddErrors(updateResult.Errors);
                    foreach (var error in updateResult.Errors)
                    {
                        this.Error(error, true);
                    }
                }
            }

            return this.View(model);
        }

        public ActionResult Index() => this.View();

        public ActionResult NConfig() => this.View();

        public ActionResult Versions() => this.View();

        public ActionResult AppUpdates()
        {
            var folder = clientApkProvider.ApkClientsFolder();
            var appFiles = Directory.EnumerateFiles(folder);

            return View(appFiles
                .Select(app => new FileInfo(app))
                .OrderBy(fi => fi.Name)
                .Select(fi =>
                {
                    int? version = null;
                    if (fi.Name.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
                    {
                        version = this.androidPackageReader.Read(fi.FullName)?.Version;
                    }

                    return $"<strong>{fi.Name}{(version.HasValue ? $" [ver. {version}]" : "")}</strong> ({fi.Length.Bytes().ToString("0.00")}) {fi.LastWriteTimeUtc}";
                }).ToList());
        }

        public ActionResult Settings()
        {
            var model = new SettingsModel();
            model.Settings = this.settingsProvider.GetSettings().OrderBy(setting => setting.Name);
            model.ExportSettings = this.exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey);

            return this.View(model);
        }

        [Localizable(false)]
        public ActionResult RepeatLastInterviewStatus(Guid? interviewId)
        {
            if (!interviewId.HasValue)
            {
                return this.View();
            }
            else
            {
                if (this.Request.Form["repeat"] != null)
                {
                    try
                    {
                        this.CommandService.Execute(new RepeatLastInterviewStatus(interviewId.Value, Strings.ControlPanelController_RepeatLastInterviewStatus));
                    }
                    catch (Exception exception)
                    {
                        Logger.Error($"Exception while repating last interview status: {interviewId}", exception);
                        return this.View(model: $"Error occurred on status repeating for interview {interviewId.Value.FormatGuid()}");
                    }

                    return this.View(model:
                        $"Successfully repeated status for interview {interviewId.Value.FormatGuid()}");
                }
            }

            return this.View();
        }

        #region interview ravalidationg

        public class ImportBrokenPackageModel
        {
            [Required(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.MandatoryField))]
            public int AssignmentId { get; set; }
            [Required(ErrorMessageResourceType = typeof(FieldsAndValidations), ErrorMessageResourceName = nameof(FieldsAndValidations.MandatoryField))]
            public HttpPostedFileBase PackageFile { get; set; }
        }

        [HttpGet]
        [Localizable(false)]
        public ActionResult ImportBrokenPackage()
        {
            return this.View();
        }

        [HttpPost]
        [Localizable(false)]
        public ActionResult ImportBrokenPackage(ImportBrokenPackageModel package)
        {
//#if !DEBUG
//            throw new AggregateException("Unsupported handler");
//#endif
            if (package == null)
                return this.View();

            if (!ModelState.IsValid)
                return this.View();

            bool doesGenerateNewInterviewId = false;

            var assignment = assignmentsService.GetAssignment(package.AssignmentId);
            var responsibleId = assignment.ResponsibleId;
            var user = userViewFactory.GetUser(new UserViewInputModel(responsibleId));
            if (!user.IsInterviewer())
                throw new ArgumentException("assignment must be on interviewer");

            var bytes = ReadFully(package.PackageFile.InputStream);

            var events = this.serializer.Deserialize<AggregateRootEvent[]>(bytes);
            var lastStatusChange = events.Last(e => e.Payload is InterviewStatusChanged);
            var interviewId = doesGenerateNewInterviewId ? Guid.NewGuid() : lastStatusChange.EventSourceId;
            var lastStatus = ((InterviewStatusChanged)lastStatusChange.Payload).Status;

            var newEvents = ChangeInterviewDataToCurrentEnvironment(events, doesGenerateNewInterviewId, interviewId, assignment, user.Supervisor.Id);
            var newEventStream = this.serializer.Serialize(newEvents);

            var interviewPackage = new InterviewPackage()
            {
                InterviewId = interviewId,
                QuestionnaireId = assignment.QuestionnaireId.QuestionnaireId,
                QuestionnaireVersion = assignment.QuestionnaireId.Version,
                InterviewStatus = lastStatus,
                ResponsibleId = responsibleId,
                IsCensusInterview = true,
                IncomingDate = DateTime.UtcNow,
                Events = newEventStream
            };

            interviewPackagesService.ProcessPackage(interviewPackage);

            return this.View();
        }

        private AggregateRootEvent[] ChangeInterviewDataToCurrentEnvironment(AggregateRootEvent[] events,
            bool regenerateEventId, Guid interviewId, Assignment assignment, Guid supervisorId)
        {
            List<AggregateRootEvent> newEvents = new List<AggregateRootEvent>();

            foreach (var aggregateRootEvent in events)
            {
                switch (aggregateRootEvent.Payload)
                {
                    case InterviewCreated interviewCreated:
                        aggregateRootEvent.Payload = new InterviewCreated(
                            userId: assignment.ResponsibleId,
                            questionnaireId: assignment.QuestionnaireId.QuestionnaireId,
                            questionnaireVersion: assignment.QuestionnaireId.Version,
                            assignmentId: assignment.Id,
                            isAudioRecordingEnabled: interviewCreated.IsAudioRecordingEnabled,
                            originDate: interviewCreated.OriginDate ?? aggregateRootEvent.EventTimeStamp,
                            usesExpressionStorage: interviewCreated.UsesExpressionStorage);
                        break;
                    case InterviewerAssigned interviewerAssigned:
                        aggregateRootEvent.Payload = new InterviewerAssigned(
                            userId: assignment.ResponsibleId,
                            interviewerId: assignment.ResponsibleId,
                            originDate: interviewerAssigned.OriginDate ?? aggregateRootEvent.EventTimeStamp);
                        break;
                    case SupervisorAssigned supervisorAssigned:
                        aggregateRootEvent.Payload = new SupervisorAssigned(
                            userId: assignment.ResponsibleId,
                            supervisorId: supervisorId,
                            originDate: supervisorAssigned.OriginDate ?? aggregateRootEvent.EventTimeStamp);
                        break;
                    default:
                        if (aggregateRootEvent.Payload is InterviewActiveEvent interviewActiveEvent)
                            typeof(InterviewActiveEvent)
                                .GetProperty("UserId")
                                .GetSetMethod(true).Invoke(interviewActiveEvent, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { assignment.ResponsibleId }, null);
                        break;
                }

                if (regenerateEventId)
                    aggregateRootEvent.EventIdentifier = Guid.NewGuid();

                aggregateRootEvent.EventSourceId = interviewId;
                newEvents.Add(aggregateRootEvent);
            }

            return newEvents.ToArray();
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public class RevalidateModel
        {
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
        }

        public ActionResult RevalidateInterviews()
        {
            return this.View();
        }

        [HttpGet]
        public ActionResult ReevaluateInterview()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ReevaluateInterview(Guid? interviewId)
        {
            if (!interviewId.HasValue)
            {
                return this.HttpNotFound();
            }
            else
            {
                try
                {
                    this.CommandService.Execute(new ReevaluateInterview(interviewId.Value, this.currentUser.Id));
                }
                catch (Exception exception)
                {
                        Logger.Error($"Exception while reevaluatng: {interviewId}", exception);
                        return new HttpStatusCodeResult(HttpStatusCode.NotAcceptable, exception.Message);
                }
            }
            
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        #endregion

        public ActionResult SynchronizationLog() => this.View();

        public ActionResult BrokenInterviewPackages() => this.View();
        public ActionResult RejectedInterviewPackages() => this.View();

        public Task Exceptions() => ExceptionalModule.HandleRequestAsync(System.Web.HttpContext.Current);
    }
}
