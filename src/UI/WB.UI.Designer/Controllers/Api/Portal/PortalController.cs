using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Code;
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

        public PortalController(UserManager<DesignerIdentityUser> accountRepository, 
            IQuestionnaireHelper questionnaireHelper)
        {
            this.accountRepository = accountRepository;
            this.questionnaireHelper = questionnaireHelper;
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
            (
                id : user.Id,
                login : user.UserName,
                email : user.Email,
                roles : roles.ToArray(),
                fullName : fullName
            ));
        }

        [Route("{userId}/questionnaires")]
        [HttpGet]
        public async Task<IActionResult> QuestionnairesForUser(string userId, string filter)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            var account = await this.accountRepository.FindByNameAsync(userId);

            var questionnaires = questionnaireHelper.GetQuestionnaires(
                viewer: account,
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
    }
}
