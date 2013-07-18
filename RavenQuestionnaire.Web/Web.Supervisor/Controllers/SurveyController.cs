using System.Collections.Generic;
using Core.Supervisor.Views;
using Core.Supervisor.Views.Status;
using Core.Supervisor.Views.Summary;
using Core.Supervisor.Views.Survey;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Web.Mvc;

    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;
    using Web.Supervisor.Models;

    [Authorize(Roles = "Supervisor")]
    public class SurveyController : BaseController
    {
        private readonly IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory;
        private readonly IViewFactory<SummaryTemplatesInputModel, SummaryTemplatesView> summaryTemplatesViewFactory;

        public SurveyController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
            IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory,
            IViewFactory<SummaryTemplatesInputModel, SummaryTemplatesView> summaryTemplatesViewFactory)
            : base(commandService, provider, logger)
        {
            this.surveyUsersViewFactory = surveyUsersViewFactory;
            this.summaryTemplatesViewFactory = summaryTemplatesViewFactory;
        }

        public ActionResult Index()
        {
            ViewBag.ActivePage = MenuItem.Surveys;
            return
                this.View(
                    this.surveyUsersViewFactory.Load(new SurveyUsersViewInputModel(this.GlobalInfo.GetCurrentUser().Id,
                        ViewerStatus.Supervisor)).Items);
        }

        public ActionResult Interviews()
        {
            ViewBag.ActivePage = MenuItem.Docs;
            return this.View(Filters());
        }

        public ActionResult GotoBrowser()
        {
            return this.RedirectToAction("Index");
        }

        public ActionResult Summary()
        {
            ViewBag.ActivePage = MenuItem.Summary;
            return this.View(this.summaryTemplatesViewFactory.Load(new SummaryTemplatesInputModel(
                this.GlobalInfo.GetCurrentUser().Id, ViewerStatus.Supervisor)).Items);
        }

        public ActionResult Status()
        {
            ViewBag.ActivePage = MenuItem.Statuses;
            return this.View(SurveyStatusViewItems());
        }

        private DocumentFilter Filters()
        {
            var statuses = SurveyStatusViewItems();
            var viewerId = this.GlobalInfo.GetCurrentUser().Id;
            var viewerStatus = ViewerStatus.Supervisor;

            return new DocumentFilter()
            {
                Users = this.surveyUsersViewFactory.Load(new SurveyUsersViewInputModel(this.GlobalInfo.GetCurrentUser().Id,
                        ViewerStatus.Supervisor)).Items,
                Responsibles =
                    this.surveyUsersViewFactory.Load(new SurveyUsersViewInputModel(viewerId, viewerStatus)).Items,
                Templates =
                    this.summaryTemplatesViewFactory.Load(new SummaryTemplatesInputModel(viewerId, viewerStatus)).Items,
                Statuses = statuses
            };
        }

        private IEnumerable<SurveyStatusViewItem> SurveyStatusViewItems()
        {
            var statuses = new List<SurveyStatusViewItem>()
                {
                    new SurveyStatusViewItem()
                        {
                            StatusId = SurveyStatus.Initial.PublicId,
                            StatusName = SurveyStatus.Initial.Name
                        },
                    new SurveyStatusViewItem()
                        {
                            StatusId = SurveyStatus.Redo.PublicId,
                            StatusName = SurveyStatus.Redo.Name
                        },
                    new SurveyStatusViewItem()
                        {
                            StatusId = SurveyStatus.Complete.PublicId,
                            StatusName = SurveyStatus.Complete.Name
                        },
                    new SurveyStatusViewItem()
                        {
                            StatusId = SurveyStatus.Error.PublicId,
                            StatusName = SurveyStatus.Error.Name
                        },
                    new SurveyStatusViewItem()
                        {
                            StatusId = SurveyStatus.Approve.PublicId,
                            StatusName = SurveyStatus.Approve.Name
                        }
                };
            return statuses;
        }
    }
}