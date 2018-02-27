using System;
using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Designer.Code;

namespace WB.UI.Designer.Api.Portal
{
    [RoutePrefix("api/portal")]
    [ApiBasicAuth(onlyAllowedAddresses: false)]
    public class PortalController : ApiController
    {
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly IAccountRepository accountRepository;
        private readonly IQuestionnaireHelper questionnaireHelper;

        public PortalController(
            IQuestionnaireListViewFactory viewFactory, 
            IAccountRepository accountRepository, 
            IQuestionnaireHelper questionnaireHelper)
        {
            this.viewFactory = viewFactory;
            this.accountRepository = accountRepository;
            this.questionnaireHelper = questionnaireHelper;
        }

        [Route("{userId}/questionnaires")]
        [HttpGet]
        public PagedQuestionnaireCommunicationPackage QuestionnairesForUser(string userId, string filter)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            var account = this.accountRepository.GetByNameOrEmail(userId);

            var questionnaires = questionnaireHelper.GetQuestionnaires(
                viewerId: account.ProviderUserKey,
                isAdmin: false, 
                type: QuestionnairesType.My, 
                folderId: null, 
                pageIndex: 1,
                sortBy: null, 
                sortOrder: null, 
                searchFor: filter);

           

            return new PagedQuestionnaireCommunicationPackage
            {
                TotalCount = questionnaires.TotalCount,
                Items = questionnaires.Select(questionnaireListItem =>
                    new QuestionnaireListItem
                    {
                        Id = Guid.Parse(questionnaireListItem.Id),
                        Title = questionnaireListItem.Title
                    }).ToList()
            };
        }
    }
}
