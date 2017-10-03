using System;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Code
{
    public class QuestionnaireHelper : IQuestionnaireHelper
    {
        private readonly IQuestionnaireListViewFactory viewFactory;

        public QuestionnaireHelper(
            IQuestionnaireListViewFactory viewFactory)
        {
            this.viewFactory = viewFactory;
        }

        public IPagedList<QuestionnaireListViewModel> GetQuestionnaires(Guid viewerId, bool isAdmin, bool showPublic,
            int? pageIndex = null, string sortBy = null, int? sortOrder = null, string searchFor = null)
        {
            QuestionnaireListView model = this.viewFactory.Load(new QuestionnaireListInputModel
            {
                ViewerId = viewerId,
                IsPublic = showPublic,
                IsAdminMode = isAdmin,
                Page = pageIndex ?? 1,
                PageSize = GlobalHelper.GridPageItemsCount,
                Order = sortBy,
                SearchFor = searchFor
            });

            return model.Items.Select(x => this.GetQuestionnaire(x, viewerId, isAdmin, showPublic))
                .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
        }

        public IPagedList<QuestionnaireListViewModel> GetQuestionnairesByViewerId(Guid viewerId, bool isAdmin) 
            => this.GetQuestionnaires(viewerId: viewerId, isAdmin: isAdmin, showPublic: false);

        private QuestionnaireListViewModel GetQuestionnaire(QuestionnaireListViewItem x, Guid viewerId, bool isAdmin, bool showPublic)
            => new QuestionnaireListViewModel
            {
                Id = x.PublicId.FormatGuid(),
                CreationDate = x.CreationDate,
                LastEntryDate = x.LastEntryDate,
                Title = x.Title,
                IsDeleted = x.IsDeleted,
                IsPublic = showPublic,
                CanDelete = x.CreatedBy == viewerId && !x.IsDeleted,
                CanExport = true,
                CanOpen = (showPublic || x.CreatedBy == viewerId || x.SharedPersons.Any(s => s.UserId == viewerId)) && !x.IsDeleted,
                CanSynchronize = isAdmin,
                CanExportToPdf = true,
                Owner = x.CreatedBy == null
                    ? GlobalHelper.EmptyString
                    : (x.CreatedBy == viewerId ? QuestionnaireController.You : x.CreatorName)
            };
    }
}