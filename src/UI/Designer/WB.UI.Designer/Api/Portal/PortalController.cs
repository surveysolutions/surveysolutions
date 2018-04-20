using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api.Attributes;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;

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

        [Route("user/{userId}")]
        [HttpGet]
        public PortalUserModel GetUserInfo(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) 
                
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest){ ReasonPhrase = $"Param {nameof(userId)} is empty or missing" });

            IMembershipAccount account = this.accountRepository.GetByNameOrEmail(userId);

            if (account == null)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));

            var roles = Roles.GetRolesForUser(account.UserName);

            return new PortalUserModel
            {
                Id = account.ProviderUserKey,
                Login = account.UserName,
                Email = account.Email,
                Roles = roles
            };
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
