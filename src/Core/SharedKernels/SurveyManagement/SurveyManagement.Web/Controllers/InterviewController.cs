using System;
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
            //InterviewDetailsView view = interviewDetailsViewFactory.GetInterviewDetails(id);
            //var interviewDetailsView = new NewInterviewDetailsView()
            //{
            //    InterviewInfo = new InterviewInfoModel()
            //    {
            //        id = view.PublicKey.ToString(),
            //        questionnaireId = view.QuestionnairePublicKey.ToString(),
            //        title = view.Title,
            //        status = view.Status.ToLocalizeString(),
            //        responsible = view.Responsible != null ? view.Responsible.Name : null
            //    },
            //    Groups = view.Groups.Select(@group => new GroupModel(@group.ParentId)
            //    {
            //        id = @group.Id.ToString(),
            //        depth = @group.Depth,
            //        title = @group.Title,
            //        rosterVector = @group.RosterVector,
            //        entities = new List<EntityModel>(@group.Entities.Select(entity => this.SelectModelByEntity(@group, entity)))
            //    });

            var detailsStatisticView = new DetailsStatisticView()
            {
                AnsweredCount = interviewDetailsView.Groups.Sum(_ => _.Entities.Count(x => ((x is InterviewQuestionView) && ((InterviewQuestionView)x).IsAnswered))),
                CommentedCount = interviewDetailsView.Groups.Sum(_ => _.Entities.Count(x => ((x is InterviewQuestionView) && ((InterviewQuestionView)x).Comments != null && ((InterviewQuestionView)x).Comments.Any()))),
                EnabledCount = interviewDetailsView.Groups.Sum(_ => _.Entities.Count(x => ((x is InterviewQuestionView) && ((InterviewQuestionView)x).IsEnabled))),
                FlaggedCount = interviewDetailsView.Groups.Sum(_ => _.Entities.Count(x => ((x is InterviewQuestionView) && ((InterviewQuestionView)x).IsFlagged))),
                InvalidCount = interviewDetailsView.Groups.Sum(_ => _.Entities.Count(x => ((x is InterviewQuestionView) && !((InterviewQuestionView)x).IsValid))),
                SupervisorsCount = interviewDetailsView.Groups.Sum(_ => _.Entities.Count(x => ((x is InterviewQuestionView) && ((InterviewQuestionView)x).Scope == QuestionScope.Supervisor)))
            };

            var allGroups = interviewDetailsView.Groups.ToArray();

            if (currentGroupId.HasValue)
            {
                interviewDetailsView.Groups = interviewDetailsView.Groups.Where(_ => _.Id == currentGroupId.Value || _.ParentId == currentGroupId.Value).ToList();
            }
            else
            {
                foreach (var interviewGroupView in interviewDetailsView.Groups)
                {
                    if (filter.Value == InterviewDetailsFilter.Answered)
                    {
                        interviewGroupView.Entities =
                            interviewGroupView.Entities.Where(
                                x =>
                                    x is InterviewStaticTextView ||
                                    ((x is InterviewQuestionView) && ((InterviewQuestionView) x).IsAnswered)).ToList();
                    }

                    if (filter.Value == InterviewDetailsFilter.Commented)
                    {
                        interviewGroupView.Entities =
                            interviewGroupView.Entities.Where(
                                x =>
                                    x is InterviewStaticTextView ||
                                    ((x is InterviewQuestionView) && ((InterviewQuestionView) x).Comments != null &&
                                     ((InterviewQuestionView) x).Comments.Any())).ToList();
                    }

                    if (filter.Value == InterviewDetailsFilter.Enabled)
                    {
                        interviewGroupView.Entities =
                            interviewGroupView.Entities.Where(
                                x =>
                                    x is InterviewStaticTextView ||
                                    ((x is InterviewQuestionView) && ((InterviewQuestionView) x).IsEnabled)).ToList();
                    }

                    if (filter.Value == InterviewDetailsFilter.Flagged)
                    {
                        interviewGroupView.Entities =
                            interviewGroupView.Entities.Where(
                                x =>
                                    x is InterviewStaticTextView ||
                                    ((x is InterviewQuestionView) && ((InterviewQuestionView) x).IsFlagged)).ToList();
                    }

                    if (filter.Value == InterviewDetailsFilter.Invalid)
                    {
                        interviewGroupView.Entities =
                            interviewGroupView.Entities.Where(
                                x =>
                                    x is InterviewStaticTextView ||
                                    ((x is InterviewQuestionView) && !((InterviewQuestionView) x).IsValid)).ToList();
                    }

                    if (filter.Value == InterviewDetailsFilter.Supervisors)
                    {
                        interviewGroupView.Entities =
                            interviewGroupView.Entities.Where(
                                x =>
                                    x is InterviewStaticTextView ||
                                    ((x is InterviewQuestionView) &&
                                     ((InterviewQuestionView) x).Scope == QuestionScope.Supervisor)).ToList();
                    }
                }

                interviewDetailsView.Groups = interviewDetailsView.Groups.Where(_ => _.Entities.Any()).ToList();
            }


            return
                View(new DetailsViewModel()
                {
                    Filter = filter.Value,
                    FilteredInterviewDetails = interviewDetailsView,
                    Groups = allGroups,
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