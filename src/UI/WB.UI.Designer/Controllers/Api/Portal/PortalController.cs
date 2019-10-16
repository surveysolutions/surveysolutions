using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.AllowedAddresses;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Attributes;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Api.Portal
{
    [Route("api/portal")]
    [Authorize]
    public class PortalController : ControllerBase
    {
        private readonly UserManager<DesignerIdentityUser> accountRepository;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IAllowedAddressService allowedAddressService;

        public PortalController(UserManager<DesignerIdentityUser> accountRepository, 
            IQuestionnaireHelper questionnaireHelper,
            IAllowedAddressService allowedAddressService)
        {
            this.accountRepository = accountRepository;
            this.questionnaireHelper = questionnaireHelper;
            this.allowedAddressService = allowedAddressService;
        }

        [Route("user/{userId}")]
        [HttpGet]
        public async Task<IActionResult> GetUserInfo(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new {ReasonPhrase = $"Param {nameof(userId)} is empty or missing"});

            var user = await this.accountRepository.FindByEmailAsync(userId) ?? (await this.accountRepository.FindByNameAsync(userId));

            if (user == null)
                return NotFound();

            var roles = await this.accountRepository.GetRolesAsync(user);
            var fullName = await this.accountRepository.GetFullName(user.Id);
            return Ok(new PortalUserModel
            {
                Id = user.Id,
                Login = user.UserName,
                Email = user.Email,
                Roles = roles.ToArray(),
                FullName = fullName
            });
        }

        [Route("{userId}/questionnaires")]
        [HttpGet]
        public async Task<IActionResult> QuestionnairesForUser(string userId, string filter)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            var account = await this.accountRepository.FindByNameAsync(userId);

            var questionnaires = questionnaireHelper.GetQuestionnaires(
                viewerId: account.Id,
                isAdmin: false, 
                type: QuestionnairesType.My | QuestionnairesType.Shared, 
                folderId: null, 
                pageIndex: 1,
                sortBy: null, 
                sortOrder: null, 
                searchFor: filter);

            var result = new PagedQuestionnaireCommunicationPackage
            {
                TotalCount = questionnaires.TotalCount,
                Items = questionnaires.Select(questionnaireListItem =>
                    new QuestionnaireListItem
                    {
                        Id = Guid.Parse(questionnaireListItem.Id),
                        Title = questionnaireListItem.Title
                    }).ToList()
            };
            return Ok(result);
        }

        [Route("servers")]
        [HttpGet]
        public IActionResult GetServers() => Ok(this.allowedAddressService.GetAddresses().Select(ToAllowedAddress));

        [Route("servers/add")]
        [HttpPost]
        public IActionResult AddServer(string ipAddress, string description)
        {
            this.allowedAddressService.Add(new AllowedAddress
            {
                Description = description,
                Address = ipAddress
            });

            return Ok();
        }

        [Route("servers/delete")]
        [HttpPost]
        public IActionResult DeleteServer(string ipAddress)
        {
            var address = this.allowedAddressService.GetAddresses().FirstOrDefault(x => Equals(x.Address, ipAddress));
            if (address != null)
                this.allowedAddressService.Remove(address.Id);
            return Ok();
        }

        private ServerAddress ToAllowedAddress(AllowedAddress address)
            => new ServerAddress
            {
                Id = address.Id,
                Description = address.Description,
                Address = address.Address
            };

        public class ServerAddress
        {
            public int Id { get; set; }
            public string Description { get; set; }
            public string Address { get; set; }
        }
    }
}
