using System;
using System.Collections.Generic;
using System.Web.Http;
using Core.Supervisor.Views;
using Core.Supervisor.Views.Interview;
using Core.Supervisor.Views.Summary;
using Core.Supervisor.Views.Survey;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    public class InterviewApiController : BaseApiController
    {
        private readonly IViewFactory<InterviewInputModel, InterviewView> interviewViewFactory;

        public InterviewApiController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IViewFactory<InterviewInputModel, InterviewView> interviewViewFactory)
            : base(commandService, globalInfo, logger)
        {
            this.interviewViewFactory = interviewViewFactory;
        }

        public InterviewView Interviews(DocumentListViewModel data)
        {
            var input = new InterviewInputModel(viewerId: this.GlobalInfo.GetCurrentUser().Id,
                viewerStatus: this.GlobalInfo.IsHeadquarter ? ViewerStatus.Headquarter : ViewerStatus.Supervisor)
            {
                Orders = data.SortOrder
            };
            
            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            if (data.Request != null)
            {
                input.TemplateId = data.Request.TemplateId;
                input.ResponsibleId = data.Request.ResponsibleId;
                input.StatusId = data.Request.StatusId;
                input.OnlyNotAssigned = data.Request.OnlyAssigned;
            }

            return this.interviewViewFactory.Load(input);
        }

        [HttpPost]
        public DeleteInterviewResult DeleteInterview(DeleteInterviewsModel model)
        {
            var blockedInterviews = new List<Guid>();
            if (model != null)
            {
                var responsibleId = this.GlobalInfo.GetCurrentUser().Id;
                foreach (var interviewId in model.Interviews)
                {
                    try
                    {
                        this.CommandService.Execute(new DeleteInterviewCommand(interviewId: interviewId,
                            deletedBy: responsibleId));
                    }
                    catch
                    {
                        blockedInterviews.Add(interviewId);
                    }

                }
            }
            return new DeleteInterviewResult() {BlockedInterviews = blockedInterviews};
        }

        public class DeleteInterviewResult
        {
            public IEnumerable<Guid> BlockedInterviews { get; set; }
        }

        public void Assign()
        {
            //UserLight responsible = null;
            //CompleteQuestionnaireStatisticView stat = null;

            //var user = this.userViewFactory.Load(new UserViewInputModel(value));
            //stat = this.completeQuestionnaireStatisticViewFactory.Load(new CompleteQuestionnaireStatisticViewInputModel(cqId) { Scope = QuestionScope.Supervisor });
            //responsible = (user != null) ? new UserLight(user.PublicKey, user.UserName) : new UserLight();

            //this.CommandService.Execute(new ChangeAssignmentCommand(cqId, responsible));

            //if (stat.Status.PublicId == SurveyStatus.Unassign.PublicId)
            //{
            //    this.CommandService.Execute(
            //        new ChangeStatusCommand()
            //        {
            //            CompleteQuestionnaireId = cqId,
            //            Status = SurveyStatus.Initial,
            //            Responsible = responsible
            //        });
            //}

            //if (Request.IsAjaxRequest())
            //{
            //    return this.Json(
            //            new
            //            {
            //                status = "ok",
            //                userId = responsible.Id,
            //                userName = responsible.Name,
            //                cqId = cqId,
            //                statusName = stat.Status.Name,
            //                statusId = stat.Status.PublicId
            //            },
            //            JsonRequestBehavior.AllowGet);
            //}

            //return this.RedirectToAction("Documents", "Survey", new { id = tmptId });
        }

        
    }
}
