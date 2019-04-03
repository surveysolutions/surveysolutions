﻿using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WB.UI.Designer1.Extensions;
using WB.UI.Designer1.Models;

namespace WB.UI.Designer.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IPublicFoldersStorage publicFoldersStorage;

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Index(int? p, string sb, int? so, string f)
            => this.View(this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f, type: QuestionnairesType.My, folderId: null));

        public ActionResult Public(int? p, string sb, int? so, string f, Guid? id)
        {
            var questionnaires = this.GetQuestionnaires(pageIndex: p, sortBy: sb, sortOrder: so, searchFor: f,
                type: QuestionnairesType.Public, folderId: id);

            var folderPath = publicFoldersStorage.GetFoldersPath(folderId: id);
            var breadcrumbs = folderPath.Select(folder => new FolderBreadcrumbsModel()
            {
                FolderId = folder.PublicId,
                Title = folder.Title
            }).ToArray();

            var model = new QuestionnaireListModel()
            {
                IsSupportAssignFolders = User.IsAdmin(),
                CurrentFolderId = id,
                Breadcrumbs = breadcrumbs,
                Questionnaires = questionnaires
            };

            return this.View(model);
        }

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
