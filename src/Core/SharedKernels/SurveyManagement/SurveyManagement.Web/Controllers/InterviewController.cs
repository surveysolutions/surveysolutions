using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.Revalidate;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize(Roles = "Headquarter, Supervisor")]
    public class InterviewController : BaseController
    {
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;
        private readonly IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory;
        private readonly IInterviewHistoryFactory interviewHistoryViewFactory;
        private readonly IInterviewSummaryViewFactory interviewSummaryViewFactory;
        private readonly IInterviewDetailsViewFactory interviewDetailsViewFactory;

        public InterviewController(
            ICommandService commandService, 
            IGlobalInfoProvider provider, 
            ILogger logger,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
            IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory,
            IInterviewSummaryViewFactory interviewSummaryViewFactory,
            IInterviewHistoryFactory interviewHistoryViewFactory, 
            IInterviewDetailsViewFactory interviewDetailsViewFactory)
            : base(commandService, provider, logger)
        {
            this.changeStatusFactory = changeStatusFactory;
            this.revalidateInterviewViewFactory = revalidateInterviewViewFactory;
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
            this.interviewDetailsViewFactory = interviewDetailsViewFactory;
        }

        public ActionResult Details(Guid id, InterviewDetailsFilter? filter, Guid? currentGroupId)
        {
            this.ViewBag.ActivePage = MenuItem.Docs;

            filter = filter.HasValue ? filter : InterviewDetailsFilter.All;


            InterviewSummary interviewSummary = this.interviewSummaryViewFactory.Load(id);

            bool isAccessAllowed =
                this.GlobalInfo.IsHeadquarter ||
                (this.GlobalInfo.IsSurepvisor && this.GlobalInfo.GetCurrentUser().Id == interviewSummary.TeamLeadId);

            if (!isAccessAllowed)
                return HttpNotFound();

            ChangeStatusView interviewInfo =
                this.changeStatusFactory.Load(new ChangeStatusInputModel() {InterviewId = id});

            if (interviewInfo == null || interviewSummary == null)
                return HttpNotFound();

            ChangeStatusView interviewHistoryOfStatuses =
                this.changeStatusFactory.Load(new ChangeStatusInputModel {InterviewId = id});
            InterviewDetailsView interviewDetailsView = interviewDetailsViewFactory.GetInterviewDetails(id);

            var detailsStatisticView = new DetailsStatisticView()
            {
                AnsweredCount = interviewDetailsView.Groups.Sum(_ => _.Entities.Count(x => ((x is InterviewQuestionView) && ((InterviewQuestionView)x).IsAnswered))),
                CommentedCount = interviewDetailsView.Groups.Sum(_ => _.Entities.Count(x => ((x is InterviewQuestionView) && ((InterviewQuestionView)x).Comments != null && ((InterviewQuestionView)x).Comments.Any()))),
                EnabledCount = interviewDetailsView.Groups.Sum(_ => _.Entities.Count(x => ((x is InterviewQuestionView) && ((InterviewQuestionView)x).IsEnabled))),
                FlaggedCount = interviewDetailsView.Groups.Sum(_ => _.Entities.Count(x => ((x is InterviewQuestionView) && ((InterviewQuestionView)x).IsFlagged))),
                InvalidCount = interviewDetailsView.Groups.Sum(_ => _.Entities.Count(x => ((x is InterviewQuestionView) && !((InterviewQuestionView)x).IsValid))),
                SupervisorsCount = interviewDetailsView.Groups.Sum(_ => _.Entities.Count(x => ((x is InterviewQuestionView) && ((InterviewQuestionView)x).Scope == QuestionScope.Supervisor)))
            };

            var selectedGroups = new List<InterviewGroupView>();

            foreach (var interviewGroupView in interviewDetailsView.Groups)
            {
                if (currentGroupId.HasValue)
                {
                    if (interviewGroupView.Id == currentGroupId.Value || selectedGroups.Any(_ => _.Id == interviewGroupView.ParentId))
                    {
                        selectedGroups.Add(interviewGroupView);
                    }
                }
                else
                {
                    switch (filter)
                    {
                        case InterviewDetailsFilter.Answered:
                            interviewGroupView.Entities =
                                interviewGroupView.Entities.Where(
                                    x =>
                                        x is InterviewStaticTextView ||
                                        ((x is InterviewQuestionView) && ((InterviewQuestionView)x).IsAnswered)).ToList();
                            break;
                        case InterviewDetailsFilter.Commented:
                            interviewGroupView.Entities =
                                interviewGroupView.Entities.Where(
                                    x =>
                                        x is InterviewStaticTextView ||
                                        ((x is InterviewQuestionView) && ((InterviewQuestionView)x).Comments != null &&
                                         ((InterviewQuestionView)x).Comments.Any())).ToList();
                            break;
                        case InterviewDetailsFilter.Enabled:
                            interviewGroupView.Entities =
                            interviewGroupView.Entities.Where(
                                x =>
                                    x is InterviewStaticTextView ||
                                    ((x is InterviewQuestionView) && ((InterviewQuestionView)x).IsEnabled)).ToList();
                            break;
                        case InterviewDetailsFilter.Flagged:
                            interviewGroupView.Entities =
                            interviewGroupView.Entities.Where(
                                x =>
                                    x is InterviewStaticTextView ||
                                    ((x is InterviewQuestionView) && ((InterviewQuestionView)x).IsFlagged)).ToList();
                            break;
                        case InterviewDetailsFilter.Invalid:
                            interviewGroupView.Entities =
                            interviewGroupView.Entities.Where(
                                x =>
                                    x is InterviewStaticTextView ||
                                    ((x is InterviewQuestionView) && !((InterviewQuestionView)x).IsValid)).ToList();
                            break;
                        case InterviewDetailsFilter.Supervisors:
                            interviewGroupView.Entities =
                            interviewGroupView.Entities.Where(
                                x =>
                                    x is InterviewStaticTextView ||
                                    ((x is InterviewQuestionView) &&
                                     ((InterviewQuestionView)x).Scope == QuestionScope.Supervisor)).ToList();
                            break;
                    }

                    if (interviewGroupView.Entities.Any())
                        selectedGroups.Add(interviewGroupView);
                }
            }

            return
                View(new DetailsViewModel()
                {
                    Filter = filter.Value,
                    InterviewDetails = interviewDetailsView,
                    FilteredGroups = selectedGroups,
                    History = interviewHistoryOfStatuses,
                    Statistic = detailsStatisticView
                });
        }

        public ActionResult InterviewDetails(Guid id, string template, Guid? group, Guid? question, Guid? propagationKey)
        {
            this.ViewBag.ActivePage = MenuItem.Docs;

            ChangeStatusView interviewInfo = this.changeStatusFactory.Load(new ChangeStatusInputModel() {InterviewId = id});
            InterviewSummary interviewSummary = this.interviewSummaryViewFactory.Load(id);
            
            if (interviewInfo == null || interviewSummary == null)
                return HttpNotFound();

            bool isAccessAllowed =
                this.GlobalInfo.IsHeadquarter ||
                (this.GlobalInfo.IsSurepvisor && this.GlobalInfo.GetCurrentUser().Id == interviewSummary.TeamLeadId);

            if (!isAccessAllowed)
                return HttpNotFound();

            return
                this.View(new InterviewModel()
                {
                    InterviewId = id,
                    CurrentGroupId = group,
                    CurrentPropagationKeyId = propagationKey,
                    InterviewStatus = interviewInfo.Status
                });
        }

        public ActionResult InterviewHistory(Guid id)
        {
            return this.View(interviewHistoryViewFactory.Load(id));
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult Revalidate()
        {
            return this.View(new RevalidateModel());
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        [HttpPost]
        public ActionResult Revalidate(RevalidateModel input)
        {
            return this.RedirectToAction("ConfirmRevalidation", new { id = input.InterviewId });
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult ConfirmRevalidation(Guid id)
        {
            var model = this.revalidateInterviewViewFactory.Load(new InterviewInfoForRevalidationInputModel { InterviewId = id });
            return this.View(model);
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        [HttpPost]
        public ActionResult ConfirmRevalidation(RevalidateModel input)
        {
            this.CommandService.Execute(new ReevaluateSynchronizedInterview(input.InterviewId));
            var model = this.revalidateInterviewViewFactory.Load(new InterviewInfoForRevalidationInputModel { InterviewId = input.InterviewId });
            return this.View("ConfirmRevalidation", model);
        }
    }
}