using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
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

        public QuestionnaireListController(IPublicFoldersStorage publicFoldersStorage,
            IQuestionnaireHelper questionnaireHelper)
        {
            this.publicFoldersStorage = publicFoldersStorage;
            this.questionnaireHelper = questionnaireHelper;
        }

        public IActionResult Index() => this.RedirectToAction("My");
        
        [Route("questionnaire/my")]
        [AntiForgeryFilter]
        public ActionResult My(int? p, string sb, int? so, string f)
            => this.View(this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f, type: QuestionnairesType.My, folderId: null));

        [AntiForgeryFilter]
        [Route("questionnaire/public/{id?}")]
        public ActionResult Public(int? p, string sb, int? so, string f, Guid? id)
        {
            var questionnaires = this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f,
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
        public ActionResult Shared(int? p, string sb, int? so, string f)
            => this.View(this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f, type: QuestionnairesType.Shared, folderId: null));


        private IPagedList<QuestionnaireListViewModel> GetQuestionnaires(int? pageIndex, string sortBy, int? sortOrder, string searchFor, QuestionnairesType type, Guid? folderId)
        {
            this.SaveRequest(pageIndex: pageIndex, sortBy: ref sortBy, sortOrder: sortOrder, searchFor: searchFor, folderId: folderId);

            return this.questionnaireHelper.GetQuestionnaires(
                pageIndex: pageIndex,
                sortBy: sortBy,
                sortOrder: sortOrder,
                searchFor: searchFor,
                folderId: folderId,
                viewerId: User.GetId(),
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
