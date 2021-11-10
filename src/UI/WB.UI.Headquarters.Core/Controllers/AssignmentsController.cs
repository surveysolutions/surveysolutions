using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    [ActivePage(MenuItem.Assignments)]
    [Localizable(false)]
    public class AssignmentsController : Controller
    {
        private readonly IAuthorizedUser currentUser;
        private readonly IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private readonly IAssignmentsService assignments;
        private readonly IAssignmentViewFactory assignmentViewFactory;
        private readonly IAssignmentsImportService assignmentsImportService;
        private readonly ISampleUploadViewFactory sampleUploadViewFactory;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly AssignmentsVerificationTask assignmentsVerificationTask;
        private readonly IAssignmentsImportReader assignmentsImportReader;
        private readonly IPreloadedDataVerifier dataVerifier;
        private readonly ILogger<AssignmentsController> logger;
        private readonly IArchiveUtils archiver;
        private readonly IInvitationService invitationService;
        private readonly ICalendarEventService calendarEventService;
        private readonly IWebInterviewLinkProvider webInterviewLinkProvider;

        public AssignmentsController(IAuthorizedUser currentUser, 
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory, 
            IAssignmentsService assignments, 
            IAssignmentViewFactory assignmentViewFactory,
            IAssignmentsImportService assignmentsImportService,
            ISampleUploadViewFactory sampleUploadViewFactory,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IFileSystemAccessor fileSystemAccessor,
            IQuestionnaireStorage questionnaireStorage,
            AssignmentsVerificationTask assignmentsVerificationTask,
            IAssignmentsImportReader assignmentsImportReader,
            IPreloadedDataVerifier dataVerifier,
            ILogger<AssignmentsController> logger,
            IArchiveUtils archiver,
            IInvitationService invitationService, 
            ICalendarEventService calendarEventService, 
            IWebInterviewLinkProvider webInterviewLinkProvider)
        {
            this.currentUser = currentUser;
            this.allUsersAndQuestionnairesFactory = allUsersAndQuestionnairesFactory;
            this.assignments = assignments;
            this.assignmentViewFactory = assignmentViewFactory;
            this.assignmentsImportService = assignmentsImportService;
            this.sampleUploadViewFactory = sampleUploadViewFactory;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireStorage = questionnaireStorage;
            this.assignmentsVerificationTask = assignmentsVerificationTask;
            this.assignmentsImportReader = assignmentsImportReader;
            this.dataVerifier = dataVerifier;
            this.logger = logger;
            this.archiver = archiver;
            this.invitationService = invitationService;
            this.calendarEventService = calendarEventService;
            this.webInterviewLinkProvider = webInterviewLinkProvider;
        }
        
        [ActivePage(MenuItem.Assignments)]
        [HttpGet]
        [Route("{controller}/{id}")]
        [Route("{controller}/{action=Index}")]
        public IActionResult Index(int? id = null)
        {
            if (id.HasValue) return GetAssignmentDetails(id.Value);

            var questionnaires = this.allUsersAndQuestionnairesFactory.GetQuestionnaireComboboxViewItems();

            return View(new  
            {
                IsSupervisor = this.currentUser.IsSupervisor,
                IsObserver = this.currentUser.IsObserver,
                IsObserving = this.currentUser.IsObserving,
                IsHeadquarter = this.currentUser.IsHeadquarter || this.currentUser.IsAdministrator,
                Questionnaires = questionnaires,
                MaxInterviewsByAssignment = Constants.MaxInterviewsCountByAssignment,
                Api = new
                {
                    Responsible = this.currentUser.IsSupervisor
                        ? Url.Action("InterviewersCombobox", "Teams")
                        : Url.Action("ResponsiblesCombobox", "Teams"),
                    Assignments = Url.Action("Get", "AssignmentsApi"),
                    Interviews = Url.Action("Index", "Interviews"),
                    AssignmentsPage = Url.Action("Index", "Assignments"),
                    Profile = Url.Action("Profile", "Interviewer"),
                    SurveySetup = this.currentUser.IsSupervisor ? "" : Url.Action("Index", "SurveySetup"),
                    AssignmentsApi = Url.Content("~/api/v1/assignments")
                }
            });
        }

        private IActionResult GetAssignmentDetails(int assignmentId)
        {
            var assignment = this.assignments.GetAssignment(assignmentId);
            if (assignment == null) 
                return NotFound();

            var calendarEvent = calendarEventService.GetActiveCalendarEventForAssignmentId(assignment.Id);
            
            return View("Details", new
            {
                Archived = assignment.Archived,
                CreatedAtUtc = assignment.CreatedAtUtc,
                Email = assignment.Email,
                Id = assignment.Id,
                IdentifyingData = this.assignmentViewFactory.GetIdentifyingColumnText(assignment).Select(x =>
                    new 
                    {
                        Id = x.Identity.ToString(),
                        Title = x.Title,
                        Answer = x.Answer
                    }),
                InterviewsNeeded = assignment.InterviewsNeeded,
                InterviewsProvided = assignment.InterviewSummaries.Count,
                IsAudioRecordingEnabled = assignment.AudioRecording,
                IsCompleted = assignment.IsCompleted,
                Password = assignment.Password,
                ProtectedVariables = assignment.ProtectedVariables,
                Quantity = assignment.Quantity,
                Questionnaire = new 
                {
                    Id = assignment.QuestionnaireId.QuestionnaireId,
                    Version = assignment.QuestionnaireId.Version,
                    Title = assignment.Questionnaire.Title
                },
                ReceivedByTabletAtUtc = assignment.ReceivedByTabletAtUtc,
                Responsible = new 
                {
                    Id = assignment.ResponsibleId,
                    Name = assignment.Responsible.Name,
                    Role = Enum.GetName(typeof(UserRoles), assignment.Responsible.RoleIds.FirstOrDefault().ToUserRole())
                        ?.ToLower()
                },
                UpdatedAtUtc = assignment.UpdatedAtUtc,
                WebMode = assignment.WebMode,
                IsHeadquarters = this.currentUser.IsAdministrator || this.currentUser.IsHeadquarter,
                Comments = assignment.Comments,
                IsArchived = assignment.Archived,
                InvitationToken = this.invitationService.GetInvitationByAssignmentId(assignment.Id)?.Token,
                CalendarEvent = calendarEvent != null
                    ? new
                    {
                        StartUtc = calendarEvent.Start.ToDateTimeUtc(),
                        StartTimezone = calendarEvent.Start.Zone.Id,
                        Comment = calendarEvent.Comment,
                        Start = calendarEvent.Start,
                        StartUtc1 = calendarEvent.Start.ToDateTimeUtc(),
                        StartTimezone1 = calendarEvent.Start.Zone.Id
                    } 
                    : null,
                LinkToWebInterviewExample = this.webInterviewLinkProvider.WebInterviewRequestLink(
                    assignment.Id.ToString(), Guid.Empty.ToString())
            });
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [Route("{controller}/{action}")]
        [ObservingNotAllowed]
        public IActionResult ImportStatus() => this.Ok(this.assignmentsImportService.GetImportStatus());

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.Questionnaires)]
        [AntiForgeryFilter]
        [Route("{controller}/{action}/{id}")]
        [Route("{controller}/{action}/{id}/{step}")]
        public ActionResult Upload(string id, string step)
        {
            if(!QuestionnaireIdentity.TryParse(id, out QuestionnaireIdentity questionnaireIdentity))
                return BadRequest("Questionnaire identity has wrong format");

            SampleUploadView sampleUploadView = this.sampleUploadViewFactory.Load(
                new SampleUploadViewInputModel(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            return this.View(new
            {
                Questionnaire = new
                {
                    Id = questionnaireIdentity.ToString(),
                    Version = questionnaireIdentity.Version,
                    Title = questionnaireInfo?.Title,
                    IdentifyingQuestions = sampleUploadView.IdentifyingQuestions,
                    HiddenQuestions = sampleUploadView.HiddenQuestions,
                    RosterSizeQuestions = sampleUploadView.RosterSizeQuestions
                },
                Api = new
                {
                    ResponsiblesUrl = Url.Action("ResponsiblesCombobox", "Teams"),
                    CreateAssignmentUrl = Url.Action("TakeNew", "HQ", new {id = questionnaireIdentity.ToString()}),
                    SimpleTemplateDownloadUrl = Url.Action("SimpleTemplateDownload", "SurveySetup", new {id = questionnaireIdentity.ToString()}),
                    TemplateDownloadUrl = Url.Action("TemplateDownload", "SurveySetup", new {id = questionnaireIdentity.ToString()}),
                    UploadUrl = Url.Action("Upload"),
                    ImportStatusUrl =  Url.Action("ImportStatus"),
                    InvalidAssignmentsUrl = Url.Action("GetInvalidAssignmentsByLastImport")
                }
            });
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.Questionnaires)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObservingNotAllowed]
        [RequestSizeLimit(500 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)]
        public async Task<IActionResult> Upload(AssignmentUploadModel model)
        {
            if (!QuestionnaireIdentity.TryParse(model.QuestionnaireId, out QuestionnaireIdentity questionnaireIdentity))
                return NotFound(model.QuestionnaireId);

            if (model.File == null)
                return BadRequest("File not found");

            if (!model.ResponsibleId.HasValue)
                return BadRequest("Responsible not found");

            var status = this.assignmentsImportService.GetImportStatus();

            if (status?.ProcessStatus == AssignmentsImportProcessStatus.Verification ||
                status?.ProcessStatus == AssignmentsImportProcessStatus.Import)
                return BadRequest("Upload assignments in progress");

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            var fileName = model.File.FileName;

            if (questionnaireInfo.IsDeleted)
                return this.Ok(CreateError(BatchUpload.Prerequisite_Questionnaire));

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            var extension = this.fileSystemAccessor.GetFileExtension(fileName).ToLower();

            var isTextFile = new[] {@".tab", @".txt"}.Contains(extension);
            var isZip = @".zip" == extension;

            if (!isTextFile && !isZip && model.Type == AssignmentUploadType.Simple)
                return this.Ok(CreateError(BatchUpload.Prerequisite_TabOrTxtFile));

            if (!isZip && model.Type == AssignmentUploadType.Advanced)
                return this.Ok(CreateError(BatchUpload.Prerequisite_ZipFile));

            PreloadedFileInfo[] allImportedFileInfos = null;
            try
            {
                if (isTextFile)
                {
                    var file = this.assignmentsImportReader.ReadTextFileInfo(model.File.OpenReadStream(), fileName);
                    file.QuestionnaireOrRosterName = questionnaire?.VariableName ?? questionnaire?.Title; /*we expect that it is main file*/

                    allImportedFileInfos = new[] {file};
                }

                if (isZip)
                    allImportedFileInfos = this.assignmentsImportReader.ReadZipFileInfo(model.File.OpenReadStream())?.ToArray();
            }
            catch (ZipException)
            {
                return this.Ok(CreateError(PreloadingVerificationMessages.ArchiveWithPasswordNotSupported));
            }

            if (allImportedFileInfos?.Any() != true)
                return this.Ok(CreateError(PreloadingVerificationMessages.PL0024_DataWasNotFound, "PL0024"));

            try
            {
                var preloadedFileInfo = this.GetSampleFile(isTextFile, isZip, allImportedFileInfos, questionnaire);

                var errors = model.Type == AssignmentUploadType.Advanced
                    ? this.dataVerifier.VerifyFiles(fileName, allImportedFileInfos, questionnaire).Take(10).ToArray()
                    : this.dataVerifier.VerifyFile(fileName, preloadedFileInfo, questionnaire).Take(10).ToArray();

                if (!errors.Any())
                {
                    errors = this.dataVerifier.VerifyColumns(allImportedFileInfos, questionnaire).Take(10).ToArray();
                    if (!errors.Any())
                    {
                        if (model.Type == AssignmentUploadType.Advanced)
                        {
                            var allImportedFiles = this.assignmentsImportReader.ReadZipFile(model.File.OpenReadStream()).ToArray();

                            var protectedFile = allImportedFiles.FirstOrDefault(x => x.FileInfo.QuestionnaireOrRosterName
                                .Equals(ServiceFiles.ProtectedVariables, StringComparison.OrdinalIgnoreCase));

                            if (protectedFile != null)
                                errors = this.dataVerifier.VerifyProtectedVariables(fileName, protectedFile, questionnaire).Take(10).ToArray();

                            if (!errors.Any())
                                errors = this.assignmentsImportService.VerifyPanelAndSaveIfNoErrors(fileName,
                                    allImportedFiles.Where(x => !x.Equals(protectedFile)).ToArray(), model.ResponsibleId.Value, protectedFile, questionnaire).Take(10).ToArray();       
                        }
                        else
                        {
                            var preloadedFile = isTextFile
                                ? this.assignmentsImportReader.ReadTextFile(model.File.OpenReadStream(), fileName)
                                : this.assignmentsImportReader.ReadFileFromZip(model.File.OpenReadStream(), preloadedFileInfo.FileName);

                            if (preloadedFile.Rows.Length == 0)
                                return this.Ok(CreateError(PreloadingVerificationMessages.PL0024_DataWasNotFound, "PL0024"));

                            errors = this.assignmentsImportService
                                .VerifySimpleAndSaveIfNoErrors(preloadedFile, model.ResponsibleId.Value, questionnaire).Take(10).ToArray();
                        }
                    }
                }

                if (errors.Any())
                    return this.Ok(errors);

                await this.assignmentsVerificationTask.ScheduleRunAsync(3);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, @"Import assignments error");

                return this.Ok(CreateError(Pages.GlobalSettings_UnhandledExceptionMessage));
            }

            return this.Ok(Array.Empty<PanelImportVerificationError>());
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        [HttpGet]
        [ObservingNotAllowed]
        [Route("{controller}/{action=Index}")]
        public IActionResult GetInvalidAssignmentsByLastImport()
        {
            var sb = new StringBuilder();
            
            foreach (var interviewImportError in this.assignmentsImportService.GetImportAssignmentsErrors())
            {
                sb.AppendLine(interviewImportError);
            }

            var invalidAssignmentsFileName = @"invalid-assignments";

            return File(this.archiver.CompressStringToByteArray($"{invalidAssignmentsFileName}.tab", sb.ToString()),
                "application/zip", $"{invalidAssignmentsFileName}.zip");
        }

        private PreloadedFileInfo GetSampleFile(bool isTextFile, bool isZip, PreloadedFileInfo[] allImportedFileInfos, IQuestionnaire questionnaire)
        {
            var questionnaireFileName = this.fileSystemAccessor.MakeStataCompatibleFileName(questionnaire.Title);

            if (isTextFile)
                return allImportedFileInfos.FirstOrDefault();

            if (isZip)
                return allImportedFileInfos.FirstOrDefault(x => x.QuestionnaireOrRosterName == questionnaireFileName) ??
                       allImportedFileInfos.FirstOrDefault(x => x.QuestionnaireOrRosterName == questionnaire.VariableName) ??
                       allImportedFileInfos.FirstOrDefault();

            return null;
        }

        private PanelImportVerificationError[] CreateError(string error, string code = null)
            => new[] {new PanelImportVerificationError(code ?? "PL0000", error)};
    }
}
