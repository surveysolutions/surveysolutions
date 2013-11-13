using Main.Core;
using Main.Core.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernel.Utils.Compression;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.Exceptions;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Api
{
    public class TesterController : ApiController
    {
        private readonly IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory;
        private readonly IMembershipUserService userHelper;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IJsonExportService exportService;
        private readonly ILogger logger;

        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;


        public TesterController(IMembershipUserService userHelper,
            IQuestionnaireHelper questionnaireHelper,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IJsonExportService exportService,
            ILogger logger)
        {
            this.userHelper = userHelper;
            this.exportService = exportService;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.logger = logger;
            this.questionnaireHelper = questionnaireHelper;
        }
        
        [Authorize]
        [HttpGet]
        public QuestionnaireListSyncPackage GetAllTemplates()
        {
            var user = this.userHelper.WebUser;

            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var questionnaireList = this.questionnaireHelper.GetQuestionnaires(
                viewerId: user.UserId);
            var questionnaireSyncPackage = new QuestionnaireListSyncPackage();

            questionnaireSyncPackage.Items = questionnaireList.Select(q => new QuestionnaireListItem(q.Id, q.Title)).ToList();

            return questionnaireSyncPackage;
        }

        [Authorize]
        [HttpPost]
        public bool Authorize()
        {
            if (this.userHelper.WebUser == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);
            return true;
        }

        [Authorize]
        [HttpGet]
        public QuestionnaireSyncPackage GetTemplate(Guid id)
        {
            var user = this.userHelper.WebUser;

            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
                return null;

            if (!ValidateAccessPermissions(questionnaireView, user.UserId))
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var templateInfo = this.exportService.GetQuestionnaireTemplate(questionnaireView.Source);
            if (templateInfo == null || string.IsNullOrEmpty(templateInfo.Source))
            {
                return null;
            }

            var template = PackageHelper.CompressString(templateInfo.Source);

            var questionnaireSyncPackage = new QuestionnaireSyncPackage(template);

            return questionnaireSyncPackage;
        }

        private bool ValidateAccessPermissions(QuestionnaireView questionnaireView, Guid currentPersonId)
        {
            if (questionnaireView.CreatedBy == currentPersonId)
                return true;

            QuestionnaireSharedPersons questionnaireSharedPersons =
                this.sharedPersonsViewFactory.Load(new QuestionnaireSharedPersonsInputModel() { QuestionnaireId = questionnaireView.PublicKey });
            
            bool isQuestionnaireIsSharedWithThisPerson = (questionnaireSharedPersons != null) && questionnaireSharedPersons.SharedPersons.Any(x => x.Id == currentPersonId);

            return isQuestionnaireIsSharedWithThisPerson;

        }
    }
}
