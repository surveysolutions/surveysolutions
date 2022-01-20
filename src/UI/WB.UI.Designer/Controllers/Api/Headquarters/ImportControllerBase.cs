using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Controllers.Api.Headquarters
{
    public class ImportControllerBase : ControllerBase
    {
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        protected readonly IDesignerEngineVersionService engineVersionService;

        public ImportControllerBase(
            IQuestionnaireListViewFactory viewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IDesignerEngineVersionService engineVersionService)
        {
            this.viewFactory = viewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.engineVersionService = engineVersionService;
        }

        public virtual void Login() { }

        protected IActionResult? CheckInvariants(int clientVersion, QuestionnaireView questionnaireView)
        {
            if (!this.engineVersionService.IsClientVersionSupported(clientVersion))
            {
                var message = string.Format(ErrorMessages.OldClientPleaseUpdate, clientVersion);
                return this.Error(StatusCodes.Status426UpgradeRequired,message);
            }

            var listOfNewFeaturesForClient = this.engineVersionService.GetListOfNewFeaturesForClient(questionnaireView.Source, clientVersion).ToList();
            if (listOfNewFeaturesForClient.Any())
            {
                var reasonPhrase = string.Format(ErrorMessages.YourQuestionnaire_0_ContainsNewFunctionalityWhichIsNotSupportedByYourInstallationPleaseUpdate,
                    questionnaireView.Title,
                    string.Join(", ", listOfNewFeaturesForClient.Select(featureDescription => $"\"{featureDescription}\"")));
                return this.Error(StatusCodes.Status403Forbidden, reasonPhrase);
            }

            var questionnaireErrors = this.questionnaireVerifier.GetAllErrors(questionnaireView).ToArray();

            if (questionnaireErrors.Any(x => x.MessageLevel > VerificationMessageLevel.Warning))
            {
                var msg = ErrorMessages.Questionnaire_verification_failed;
                return this.Error(StatusCodes.Status403Forbidden, msg);
            }

            return null;
        }

        public virtual PagedQuestionnaireCommunicationPackage PagedQuestionnaireList(QuestionnaireListRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var questionnaireListView = this.viewFactory.Load(
                new QuestionnaireListInputModel
                {

                    ViewerId = User.GetId(),
                    IsAdminMode = User.IsAdmin(),
                    Page = request.PageIndex,
                    PageSize = request.PageSize,
                    Order = request.SortOrder,
                    SearchFor = request.SearchFor
                });

            return new PagedQuestionnaireCommunicationPackage()
            {
                TotalCount = questionnaireListView.TotalCount,
                Items = questionnaireListView.Items.Select(questionnaireListItem =>
                    new QuestionnaireListItem()
                    {
                        Id = questionnaireListItem.PublicId,
                        Title = questionnaireListItem.Title
                    }).ToList()
            };
        }

  
    }
}
