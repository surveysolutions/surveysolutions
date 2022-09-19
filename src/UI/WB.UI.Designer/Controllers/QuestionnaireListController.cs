using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Filters;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Controllers
{
    [Authorize]
    public class QuestionnaireListController : Controller
    {
        private readonly IPublicFoldersStorage publicFoldersStorage;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly UserManager<DesignerIdentityUser> accountRepository;

        public QuestionnaireListController(IPublicFoldersStorage publicFoldersStorage,
            IQuestionnaireHelper questionnaireHelper,
            UserManager<DesignerIdentityUser> accountRepository)
        {
            this.publicFoldersStorage = publicFoldersStorage;
            this.questionnaireHelper = questionnaireHelper;
            this.accountRepository = accountRepository;
        }

        public IActionResult Index() => this.RedirectToAction("My");
        
        [Route("questionnaire/my")]
        [AntiForgeryFilter]
        public async Task<ActionResult> My(int? p, string sb, int? so, string f)
            => this.View(await this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f, type: QuestionnairesType.My, folderId: null));

        [AntiForgeryFilter]
        [Route("questionnaire/public/{id?}")]
        public async Task<ActionResult> Public(int? p, string sb, int? so, string f, Guid? id)
        {
            var questionnaires = await this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f,
                type: QuestionnairesType.Public, folderId: id);

            var folderPath = publicFoldersStorage.GetFoldersPath(folderId: id);
            var breadcrumbs = folderPath.Select(folder => new FolderBreadcrumbsModel
            (
                folderId : folder.PublicId,
                title : folder.Title
            )).ToArray();

            var model = new QuestionnaireListModel
            (
                isSupportAssignFolders : User.IsAdmin(),
                currentFolderId : id,
                breadcrumbs : breadcrumbs,
                questionnaires : questionnaires
            );

            return this.View(model);
        }

        [Route("questionnaire/shared")]
        [AntiForgeryFilter]
        public async Task<ActionResult> Shared(int? p, string sb, int? so, string f)
            => this.View(await this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f, type: QuestionnairesType.Shared, folderId: null));


        private async Task<IPagedList<QuestionnaireListViewModel>> GetQuestionnaires(int? pageIndex, string sortBy, int? sortOrder, string searchFor, QuestionnairesType type, Guid? folderId)
        {
            var account = await this.accountRepository.FindByIdAsync(User.GetId().FormatGuid());

            this.SaveRequest(pageIndex: pageIndex, sortBy: ref sortBy, sortOrder: sortOrder, searchFor: searchFor, folderId: folderId);

            return this.questionnaireHelper.GetQuestionnaires(
                pageIndex: pageIndex,
                sortBy: sortBy,
                sortOrder: sortOrder,
                searchFor: searchFor,
                folderId: folderId,
                viewer: account,
                isAdmin: User.IsAdmin(),
                type: type);
        }

        private void SaveRequest(int? pageIndex, ref string sortBy, int? sortOrder, string searchFor, Guid? folderId)
        {
            this.ViewBag.PageIndex = pageIndex;
            this.ViewBag.SortBy = sortBy;
            this.ViewBag.Filter = searchFor;
            this.ViewBag.SortOrder = sortOrder;
            this.ViewBag.FolderId = folderId;

            if (sortOrder.ToBool())
            {
                sortBy = $"{sortBy} Desc";
            }
        }
    }
}
