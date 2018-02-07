using System;
using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Attributes;

namespace WB.UI.Designer.Api.Portal
{
    [RoutePrefix("api/portal")]
    [ApiBasicAuth(onlyAllowedAddresses: false)]
    public class PortalController : ApiController
    {
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly IAccountRepository accountRepository;

        public PortalController(
            IQuestionnaireListViewFactory viewFactory, 
            IAccountRepository accountRepository)
        {
            this.viewFactory = viewFactory;
            this.accountRepository = accountRepository;
        }

        [Route("{userId:string}/questionnaires")]
        [HttpGet]
        public PagedQuestionnaireCommunicationPackage QuestionnairesForUser(string userId, string filter)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            var account = this.accountRepository.GetByNameOrEmail(userId);

            var input = new QuestionnaireListInputModel
            {
                ViewerId = account.ProviderUserKey,
                IsAdminMode = false,
                Page = 0,
                PageSize = 20,
                SearchFor = filter
            };

            var questionnaireListView = this.viewFactory.Load(input);

            return new PagedQuestionnaireCommunicationPackage
            {
                TotalCount = questionnaireListView.TotalCount,
                Items = questionnaireListView.Items.Select(questionnaireListItem =>
                    new QuestionnaireListItem
                    {
                        Id = questionnaireListItem.PublicId,
                        Title = questionnaireListItem.Title
                    }).ToList()
            };
        }
    }
}