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
using WB.Core.Synchronization;

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
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;

        public InterviewController(
            ICommandService commandService, 
            IGlobalInfoProvider provider, 
            ILogger logger,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
            IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory,
            IInterviewSummaryViewFactory interviewSummaryViewFactory,
            IInterviewHistoryFactory interviewHistoryViewFactory, 
            IInterviewDetailsViewFactory interviewDetailsViewFactory,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue)
            : base(commandService, provider, logger)
        {
            this.changeStatusFactory = changeStatusFactory;
            this.revalidateInterviewViewFactory = revalidateInterviewViewFactory;
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
            this.interviewDetailsViewFactory = interviewDetailsViewFactory;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
        }

        private decimal[] ParseRosterVector(string rosterVectorAsString)
        {
            if (string.IsNullOrEmpty(rosterVectorAsString))
                return new decimal[0];

            return rosterVectorAsString.Split('_').Select(vector => decimal.Parse(vector.Replace('-', '.'))).ToArray();
        }

        public ActionResult Details(Guid id, InterviewDetailsFilter? filter, Guid? currentGroupId, string rosterVector)
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

            var questionViews = interviewDetailsView.Groups.SelectMany(group => group.Entities).OfType<InterviewQuestionView>();
            var detailsStatisticView = new DetailsStatisticView()
            {
                AnsweredCount = questionViews.Count(question => question.IsAnswered),
                UnansweredCount = questionViews.Count(question => question.IsEnabled && !question.IsAnswered),
                CommentedCount = questionViews.Count(question => question.Comments != null && question.Comments.Any()),
                EnabledCount = questionViews.Count(question => question.IsEnabled),
                FlaggedCount = questionViews.Count(question => question.IsFlagged),
                InvalidCount = questionViews.Count(question => !question.IsValid),
                SupervisorsCount = questionViews.Count(question => question.Scope == QuestionScope.Supervisor)
            };

            var selectedGroups = new List<InterviewGroupView>();

            var currentGroup = interviewDetailsView.Groups.Find(group => currentGroupId != null && group.Id == currentGroupId && group.RosterVector.SequenceEqual(ParseRosterVector(rosterVector)));

            foreach (var interviewGroupView in interviewDetailsView.Groups)
            {
                if (currentGroup != null && currentGroup.ParentId.HasValue)
                {
                    if (interviewGroupView.Id == currentGroup.Id &&
                        interviewGroupView.RosterVector.SequenceEqual(currentGroup.RosterVector) ||
                        selectedGroups.Any(_ => _.Id == interviewGroupView.ParentId))
                    {
                        selectedGroups.Add(interviewGroupView);
                    }
                }
                else
                {
                    var filteredQuestions = interviewGroupView.Entities.OfType<InterviewQuestionView>();
                    switch (filter)
                    {
                        case InterviewDetailsFilter.Answered:
                            filteredQuestions = filteredQuestions.Where(question => question.IsAnswered);
                            break;
                        case InterviewDetailsFilter.Unanswered:
                            filteredQuestions = filteredQuestions.Where(question => question.IsEnabled && !question.IsAnswered);
                            break;
                        case InterviewDetailsFilter.Commented:
                            filteredQuestions = filteredQuestions.Where(question => question.Comments != null && question.Comments.Any());
                            break;
                        case InterviewDetailsFilter.Enabled:
                            filteredQuestions = filteredQuestions.Where(question => question.IsEnabled);
                            break;
                        case InterviewDetailsFilter.Flagged:
                            filteredQuestions = filteredQuestions.Where(question => question.IsFlagged);
                            break;
                        case InterviewDetailsFilter.Invalid:
                            filteredQuestions = filteredQuestions.Where(question => !question.IsValid);
                            break;
                        case InterviewDetailsFilter.Supervisors:
                            filteredQuestions = filteredQuestions.Where(question => question.Scope == QuestionScope.Supervisor);
                            break;
                    }
                    interviewGroupView.Entities = filteredQuestions.Select(x => (InterviewEntityView) x).ToList();

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
                    Statistic = detailsStatisticView,
                    HasUnprocessedSyncPackages = this.incomingSyncPackagesQueue.HasPackagesByInterviewId(id)
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