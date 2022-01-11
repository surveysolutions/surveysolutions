using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter, UserRoles.Supervisor)]
    [Localizable(false)]
    public class QuestionnairesController : Controller
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems;
        private readonly IQuestionnaireBrowseViewFactory browseViewFactory;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IPlainKeyValueStorage<QuestionnairePdf> pdfStorage;
        private readonly IUserViewFactory userViewFactory;
        private readonly IQuestionnaireVersionProvider questionnaireVersionProvider;
        private readonly IRestServiceSettings restServiceSettings;
        private readonly IAuthorizedUser authorizedUser;
        private readonly ICommandService commandService;
        private readonly ILogger<QuestionnairesController> logger;

        public QuestionnairesController(
            IQuestionnaireStorage questionnaireStorage,
            IQuestionnaireBrowseViewFactory browseViewFactory,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IRestServiceSettings restServiceSettings,
            IPlainKeyValueStorage<QuestionnairePdf> pdfStorage,
            IUserViewFactory userViewFactory, 
            IQuestionnaireVersionProvider questionnaireVersionProvider, 
            IAuthorizedUser authorizedUser, 
            ICommandService commandService,
            ILogger<QuestionnairesController> logger,
            IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems)
        {
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.browseViewFactory = browseViewFactory ?? throw new ArgumentNullException(nameof(browseViewFactory));
            this.webInterviewConfigProvider = webInterviewConfigProvider ?? throw new ArgumentNullException(nameof(webInterviewConfigProvider));
            this.restServiceSettings = restServiceSettings ?? throw new ArgumentNullException(nameof(restServiceSettings));
            this.pdfStorage = pdfStorage ?? throw new ArgumentNullException(nameof(pdfStorage));
            this.userViewFactory = userViewFactory ?? throw new ArgumentNullException(nameof(userViewFactory));
            this.questionnaireVersionProvider = questionnaireVersionProvider;
            this.authorizedUser = authorizedUser;
            this.commandService = commandService;
            this.logger = logger;
            this.questionnaireItems = questionnaireItems;
        }

        public IActionResult Details(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);
            if (questionnaire == null)
                return NotFound(string.Format(HQ.QuestionnaireNotFoundFormat, questionnaireIdentity.QuestionnaireId.FormatGuid(), questionnaireIdentity.Version));
            
            var browseItem = browseViewFactory.GetById(questionnaireIdentity);

            var model = new QuestionnaireDetailsModel
            {
                QuestionnaireId = questionnaire.QuestionnaireId,
                Title = questionnaire.Title,
                Version = questionnaire.Version,
                ImportDateUtc = browseItem.ImportDate.GetValueOrDefault(),
                LastEntryDateUtc = browseItem.LastEntryDate,
                CreationDateUtc = browseItem.CreationDate,
                WebMode = this.webInterviewConfigProvider.Get(questionnaireIdentity).Started,
                AudioAudit = browseItem.IsAudioRecordingEnabled,
                DesignerUrl = this.restServiceSettings.Endpoint.TrimEnd('/') +
                              $"/questionnaire/details/{questionnaire.QuestionnaireId:N}${questionnaire.Revision}",
                Comment = browseItem.Comment,
                Variable = browseItem.Variable,
                IsObserving = this.authorizedUser.IsObserving,
                DefaultLanguageName = questionnaire.DefaultLanguageName,
                ExposedVariablesUrl = Url.Action("ExposedVariables", "Questionnaires")
        };

            if (browseItem.ImportedBy.HasValue && browseItem.ImportedBy != Guid.Empty)
            {
                var user = this.userViewFactory.GetUser(browseItem.ImportedBy.Value);
                if (user != null)
                {
                    model.ImportedBy = new QuestionnaireDetailsModel.User
                    {
                        Role = Enum.GetName(typeof(UserRoles), user.Roles.First()),
                        Name = user.UserName
                    };
                }
            }

            var mainPdfFile = this.pdfStorage.HasNotEmptyValue(questionnaireIdentity.ToString());
            if (mainPdfFile)
            {
                model.MainPdfUrl = Url.Action("Pdf", new { id = id });
            }

            foreach (var translation in questionnaire.Translations)
            {
                if (this.pdfStorage.HasNotEmptyValue($"{translation.Id:N}_{questionnaireIdentity}"))
                {
                    var url = Url.Action("Pdf",
                        new
                        {
                            id = questionnaireIdentity.ToString(),
                            translation = translation.Id
                        });

                    var pdf = new TranslatedPdf(translation.Name, url);

                    model.TranslatedPdfVersions.Add(pdf);
                }
            }

            FillStats(questionnaireIdentity, model);

            return View(model);
        }

        [AntiForgeryFilter]
        [AuthorizeByRole(UserRoles.Administrator)]
        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult Clone(Guid id, long version)
        {
            QuestionnaireBrowseItem questionnaireBrowseItem = this.browseViewFactory.GetById(new QuestionnaireIdentity(id, version));

            if (questionnaireBrowseItem == null)
                return NotFound(string.Format(HQ.QuestionnaireNotFoundFormat, id.FormatGuid(), version));

            var cloneQuestionnaireModel = new CloneQuestionnaireModel(id, 
                version, 
                questionnaireBrowseItem.Title, 
                questionnaireBrowseItem.AllowCensusMode, 
                questionnaireBrowseItem.Comment);

            return this.View(cloneQuestionnaireModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeByRole(UserRoles.Administrator)]
        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult Clone(CloneQuestionnaireModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }
            try
            {
                var newVersion = this.questionnaireVersionProvider.GetNextVersion(model.Id);
                this.commandService.Execute(new CloneQuestionnaire(
                    model.Id, model.Version, model.NewTitle, newQuestionnaireVersion:newVersion, userId: this.authorizedUser.Id, comment: model.Comment));
            }
            catch (QuestionnaireException exception)
            {
                this.logger.LogError(exception, $"Error occurred while cloning questionnaire (id: {model?.Id}, version: {model?.Version}).");

                model.Error = exception.Message;

                return this.View(model);
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, $"Unexpected error occurred while cloning questionnaire (id: {model.Id}, version: {model.Version}).");
                model.Error = QuestionnaireClonning.UnexpectedError;
                return this.View(model);
            }

            return this.RedirectToAction("Index", "SurveySetup");
        }

        [AntiForgeryFilter]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter)]
        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult ExposedVariables(string id)
        {
            //var questionnaireIdentity = QuestionnaireIdentity.Parse(id);

            if (!QuestionnaireIdentity.TryParse(id, out QuestionnaireIdentity questionnaireIdentity))
            {
                return NotFound();
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            if (questionnaire == null)
            {
                return NotFound();
            }

            var model = new QuestionnaireExposedVariablesModel()
            {
                QuestionnaireIdentity = id,
                QuestionnaireId = questionnaireIdentity.QuestionnaireId,
                Title = questionnaire.Title,
                Version = questionnaireIdentity.Version,
                IsObserving = this.authorizedUser.IsObserving,
                DataUrl = Url.Action("GetQuestionnaireVariables", "QuestionnairesApi"),
                DesignerUrl = this.restServiceSettings.Endpoint.TrimEnd('/') +
                              $"/questionnaire/details/{questionnaire.QuestionnaireId:N}${questionnaire.Revision}",

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeByRole(UserRoles.Administrator, UserRoles.Headquarter)]
        [ActivePage(MenuItem.Questionnaires)]
        public IActionResult ExposedVariables(QuestionnaireExposedVariablesModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }
            //var model = new QuestionnaireExposedVariablesModel();
            
            return View(model);
        }

        public IActionResult Pdf(string id, Guid? translation = null)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireIdentity, null);

            var pdf = translation != null
                ? this.pdfStorage.GetById($"{translation:N}_{id}")
                : this.pdfStorage.GetById(questionnaireIdentity.ToString());

            var fileName = Path.ChangeExtension(questionnaire.VariableName, ".pdf");
            if (translation != null)
            {
                var translationName = questionnaire.Translations.First(x => x.Id == translation).Name;
                fileName = translationName + " " + fileName;
            }

            return File(pdf.Content, "application/pdf", fileName);
        }

        private void FillStats(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDetailsModel model)
        {
            var document = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);

            foreach (var questionnaireEntry in document.Children.TreeToEnumerable(x => x.Children))
            {
                if (questionnaireEntry is IGroup group)
                {
                    if (group.GetParent().PublicKey == questionnaireIdentity.QuestionnaireId)
                    {
                        model.SectionsCount++;
                    }
                    else if (group.IsRoster)
                    {
                        model.RostersCount++;
                    }
                    else
                    {
                        model.SubSectionsCount++;
                    }
                }
                else
                {
                    if (questionnaireEntry is IQuestion question)
                    {
                        model.QuestionsCount++;
                        if (!string.IsNullOrEmpty(question.ConditionExpression))
                        {
                            model.QuestionsWithConditionsCount++;
                        }
                    }
                }
            }
        }
    }
}
