using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Views.AllowedAddresses;
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
        private readonly IAccountRepository accountRepository;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IAllowedAddressService allowedAddressService;

        public PortalController(IAccountRepository accountRepository, 
            IQuestionnaireHelper questionnaireHelper,
            IAllowedAddressService allowedAddressService)
        {
            this.accountRepository = accountRepository;
            this.questionnaireHelper = questionnaireHelper;
            this.allowedAddressService = allowedAddressService;
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
                type: QuestionnairesType.My | QuestionnairesType.Shared, 
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

        [Route("servers")]
        [HttpGet]
        public IEnumerable<ServerAddress> GetServers() => this.allowedAddressService.GetAddresses().Select(ToAllowedAddress);

        [Route("servers/add")]
        [HttpPost]
        public void AddServer(string ipAddress, string description)
            => this.allowedAddressService.Add(new AllowedAddress
            {
                Description = description,
                Address = IPAddress.Parse(ipAddress)
            });

        [Route("servers/delete")]
        [HttpPost]
        public void DeleteServer(string ipAddress)
        {
            var parsedAddress = IPAddress.Parse(ipAddress);
            var address = this.allowedAddressService.GetAddresses().FirstOrDefault(x => Equals(x.Address, parsedAddress));
            if (address != null)
                this.allowedAddressService.Remove(address.Id);
        }

        private ServerAddress ToAllowedAddress(AllowedAddress address)
            => new ServerAddress
            {
                Id = address.Id,
                Description = address.Description,
                Address = address.Address.ToString()
            };

        public class ServerAddress
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public string Address { get; set; }
        }
    }
}
