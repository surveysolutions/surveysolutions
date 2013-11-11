using System.Collections.Generic;
using Main.Core;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.UI.Designer.Controllers
{
    using Main.Core.View;
    using System;
    using System.Linq;
    using System.Net;
    using System.Web.Http;
    using WB.Core.BoundedContexts.Designer.Services;
    using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
    using WB.Core.GenericSubdomains.Logging;
    using WB.Core.SharedKernel.Utils.Compression;
    using WB.UI.Shared.Web.Exceptions;
    using WB.UI.Shared.Web.Membership;


    public class TestApiController : ApiController
    {
        private readonly IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory;
        private readonly IViewFactory<QuestionnaireListViewInputModel, QuestionnaireListView> questionnaireListViewFactory;
        private readonly IMembershipUserService userHelper;
        private readonly IJsonExportService exportService;
        private readonly IStringCompressor zipUtils;
        private readonly ILogger logger;

        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;


        public TestApiController(IMembershipUserService userHelper, 
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory,
            IViewFactory<QuestionnaireListViewInputModel, QuestionnaireListView> viewFactory,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IJsonExportService exportService,
            IStringCompressor zipUtils,
            ILogger logger)
        {
            this.userHelper = userHelper;
            this.questionnaireListViewFactory = viewFactory;
            this.exportService = exportService;
            this.zipUtils = zipUtils;
            this.sharedPersonsViewFactory = sharedPersonsViewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.logger = logger;
        }
        
        [Authorize]
        public IEnumerable<QuestionnaireListViewItem> GetAllTemplates()
        {
            // change to other return type
            var user = this.userHelper.WebUser;

            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var questionnaireList = this.questionnaireListViewFactory.Load(
                        new QuestionnaireListViewInputModel
                            {
                                ViewerId = user.UserId,
                                IsAdminMode = false,
                            });

            return questionnaireList.Items;
        }

        [Authorize]
        public string GetTemplate(Guid id)
        {
            // change to other return type 
            var user = this.userHelper.WebUser;

            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
                return null;

            ValidateAccessPermissions(questionnaireView, user.UserId);

            var templateInfo = this.exportService.GetQuestionnaireTemplate(questionnaireView.Source);
            if (templateInfo == null || string.IsNullOrEmpty(templateInfo.Source))
            {
                return null;
            }

            var template = PackageHelper.CompressString(templateInfo.Source);

            return template;
        }

        private void ValidateAccessPermissions(QuestionnaireView questionnaireView, Guid currentPersonId)
        {
            if (questionnaireView.CreatedBy == currentPersonId)
                return;

            QuestionnaireSharedPersons questionnaireSharedPersons =
                this.sharedPersonsViewFactory.Load(new QuestionnaireSharedPersonsInputModel() { QuestionnaireId = questionnaireView.PublicKey });
            
            bool isQuestionnaireIsSharedWithThisPerson = (questionnaireSharedPersons != null) && questionnaireSharedPersons.SharedPersons.Any(x => x.Id == currentPersonId);
            
            if (!isQuestionnaireIsSharedWithThisPerson)
                throw new HttpStatusException(HttpStatusCode.Forbidden);
        }
    }
}
