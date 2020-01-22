using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveySolutions.Documents;
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
            ILogger<AssignmentsController> logger)
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
        }
        
        [ActivePage(MenuItem.Assignments)]
        [Route("{controller}/{id}")]
        [Route("{controller}/{action=Index}")]
        public IActionResult Index(int? id = null)
        {
            if (id.HasValue) return GetAssignmentDetails(id.Value);

            var questionnaires = this.allUsersAndQuestionnairesFactory.GetQuestionnaireComboboxViewItems();

            var model = new AssignmentsFilters 
            {
                IsSupervisor = this.currentUser.IsSupervisor,
                IsObserver = this.currentUser.IsObserver,
                IsObserving = this.currentUser.IsObserving,
                IsHeadquarter = this.currentUser.IsHeadquarter || this.currentUser.IsAdministrator,
                Questionnaires = questionnaires,
                MaxInterviewsByAssignment = Constants.MaxInterviewsCountByAssignment
            };

            model.Api = new AssignmentsFilters.ApiEndpoints
            {
                Responsible = model.IsSupervisor
                    ? Url.Action("InterviewersCombobox", "Teams")
                    : Url.Action("ResponsiblesCombobox", "Teams"),
                Assignments = Url.Action("Get", "AssignmentsApi"),
                Interviews = Url.Action("Index", "Interviews"),
                AssignmentsPage = Url.Action("Index", "Assignments"),
                Profile = Url.Action("Profile", "Interviewer"),
                SurveySetup = model.IsSupervisor ? "" : Url.Action("Index", "SurveySetup"),
                AssignmentsApi = Url.Content("~/api/v1/assignments")
            };

            return View(model);
        }

        private IActionResult GetAssignmentDetails(int assignmentId)
        {
            var assignment = this.assignments.GetAssignment(assignmentId);
            if (assignment == null) 
                return NotFound();

            return View("Details", new AssignmentDto
            {
                Archived = assignment.Archived,
                CreatedAtUtc = assignment.CreatedAtUtc,
                Email = assignment.Email,
                Id = assignment.Id,
                IdentifyingData = this.assignmentViewFactory.GetIdentifyingColumnText(assignment).Select(x =>
                    new AssignmentIdentifyingAnswerDto
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
                Questionnaire = new AssignmentQuestionnaireDto
                {
                    Id = assignment.QuestionnaireId.QuestionnaireId,
                    Version = assignment.QuestionnaireId.Version,
                    Title = assignment.Questionnaire.Title
                },
                ReceivedByTabletAtUtc = assignment.ReceivedByTabletAtUtc,
                Responsible = new AssignmentResponsibleDto
                {
                    Id = assignment.ResponsibleId,
                    Name = assignment.Responsible.Name,
                    Role = Enum.GetName(typeof(UserRoles), assignment.Responsible.RoleIds.FirstOrDefault().ToUserRole())
                        ?.ToLower()
                },
                UpdatedAtUtc = assignment.UpdatedAtUtc,
                WebMode = assignment.WebMode,
                IsHeadquarters = this.currentUser.IsAdministrator || this.currentUser.IsHeadquarter,
                Comments = assignment.Comments
            });
        }

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObserverNotAllowed]
        public IActionResult ImportStatus() => this.Ok(this.assignmentsImportService.GetImportStatus());

        [HttpGet]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.Questionnaires)]
        [AntiForgeryFilter]
        public ActionResult Upload(string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out QuestionnaireIdentity questionnaireIdentity))
                return NotFound(id);

            SampleUploadView sampleUploadView = this.sampleUploadViewFactory.Load(
                new SampleUploadViewInputModel(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version));
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            return this.View(new
            {
                Questionnaire = new
                {
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
                    ImportStatusUrl =  Url.Action("ImportStatus")
                }
            });
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.Questionnaires)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
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
                    file.QuestionnaireOrRosterName = questionnaire.VariableName ?? questionnaire.Title; /*we expect that it is main file*/

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
                return this.Ok(CreateError(PreloadingVerificationMessages.PL0024_DataWasNotFound));

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
                            {
                                errors = this.dataVerifier.VerifyProtectedVariables(fileName, protectedFile, questionnaire).Take(10).ToArray();

                                if (!errors.Any())
                                {
                                    errors = this.assignmentsImportService.VerifyPanelAndSaveIfNoErrors(fileName,
                                        allImportedFiles.Where(x => !x.Equals(protectedFile)).ToArray(), model.ResponsibleId.Value, protectedFile, questionnaire).Take(10).ToArray();       
                                }
                            }
                        }
                        else
                        {
                            var preloadedFile = isTextFile
                                ? this.assignmentsImportReader.ReadTextFile(model.File.OpenReadStream(), fileName)
                                : this.assignmentsImportReader.ReadFileFromZip(model.File.OpenReadStream(), preloadedFileInfo.FileName);

                            if (preloadedFile.Rows.Length == 0)
                                return this.Ok(CreateError(PreloadingVerificationMessages.PL0024_DataWasNotFound));

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

        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.Questionnaires)]
        [ObserverNotAllowed]
        public ActionResult InterviewVerificationProgress()
        {
            var status = this.assignmentsImportService.GetImportStatus();
            if (status == null) return RedirectToAction("Index");

            if (status.ProcessStatus == AssignmentsImportProcessStatus.Import ||
                status.ProcessStatus == AssignmentsImportProcessStatus.ImportCompleted)
                return RedirectToAction(nameof(InterviewImportProgress));

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(status.QuestionnaireIdentity);

            return this.View(new PreloadedDataInterviewProgressModel
            {
                Status = status,
                QuestionnaireId = questionnaireInfo.QuestionnaireId,
                Version = questionnaireInfo.Version,
                QuestionnaireTitle = questionnaireInfo.Title,
            });
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.Questionnaires)]
        [ObserverNotAllowed]
        public ActionResult InterviewImportProgress()
        {
            var status = this.assignmentsImportService.GetImportStatus();
            if (status == null) return RedirectToAction("Index");

            if (status.ProcessStatus == AssignmentsImportProcessStatus.Verification)
                return RedirectToAction(nameof(InterviewVerificationProgress));

            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(status.QuestionnaireIdentity);

            return this.View(new PreloadedDataInterviewProgressModel
            {
                Status = status,
                QuestionnaireId = questionnaireInfo.QuestionnaireId,
                Version = questionnaireInfo.Version,
                QuestionnaireTitle = questionnaireInfo.Title,
            });
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.Questionnaires)]
        [HttpGet]
        public ActionResult InterviewImportIsInProgress(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var questionnaireInfo = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            var status = this.assignmentsImportService.GetImportStatus();

            return this.View(new PreloadedDataInProgressModel
            {
                Questionnaire = new PreloadedDataQuestionnaireModel
                {
                    Id = questionnaireIdentity.QuestionnaireId,
                    Version = questionnaireIdentity.Version,
                    Title = questionnaireInfo?.Title
                },
                ProcessStatus = status?.ProcessStatus
            });
        }

        private PanelImportVerificationError[] CreateError(string error)
            => new[] {new PanelImportVerificationError(@"PL0000", error)};
    }

    public class AssignmentDto
    {
        public int Id { get; set; }

        public AssignmentResponsibleDto Responsible { get; set; }

        public int? Quantity { get; set; }

        public bool Archived { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }

        public DateTime? ReceivedByTabletAtUtc { get; set; }

        public bool IsAudioRecordingEnabled { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public bool? WebMode { get; set; }

        public IEnumerable<AssignmentIdentifyingAnswerDto> IdentifyingData { get; set; }

        public AssignmentQuestionnaireDto Questionnaire { get; set; }

        public List<string> ProtectedVariables { get; set; }

        public int InterviewsProvided { get; set; }

        public int? InterviewsNeeded { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsHeadquarters { get; set; }
        public string Comments { get; set; }
        public string HistoryUrl { get; set; }
    }

    public class AssignmentQuestionnaireDto
    {
        public Guid Id { get; set; }
        public long Version { get; set; }
        public string Title { get; set; }
    }

    public class AssignmentResponsibleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }

    public class AssignmentIdentifyingAnswerDto
    {
        public string Title { get; set; }
        public string Answer { get; set; }
        public string Id { get; set; }
    }

    public class AssignmentsFilters
    {
        public bool IsHeadquarter { get; set; }
        public bool IsSupervisor { get; set; }
        public ApiEndpoints Api { get; set; }
        public bool IsObserver { get; set; }
        public bool IsObserving { get; set; }
        public List<QuestionnaireVersionsComboboxViewItem> Questionnaires { get; set; }
        public int MaxInterviewsByAssignment { get; set; }

        public class ApiEndpoints
        {
            public string Assignments { get; set; }

            public string Profile { get; set; }
            public string Interviews { get; set; }
            public string Responsible { get; set; }

            public string SurveySetup { get; set; }
            public string AssignmentsPage { get; set; }
            public string AssignmentsApi { get; set; }
        }
    }
}
