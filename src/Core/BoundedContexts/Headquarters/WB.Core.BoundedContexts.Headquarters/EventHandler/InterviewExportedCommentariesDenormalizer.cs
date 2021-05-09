using System;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.ServicesIntegration.Export;
using InterviewComment = WB.Core.BoundedContexts.Headquarters.DataExport.Views.InterviewComment;
using QuestionnaireIdentity = WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionnaireIdentity;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewExportedCommentariesDenormalizer:
        ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>,
        IUpdateHandler<InterviewSummary, InterviewApprovedByHQ>,
        IUpdateHandler<InterviewSummary, InterviewCompleted>,
        IUpdateHandler<InterviewSummary, InterviewRestarted>,
        IUpdateHandler<InterviewSummary, InterviewApproved>,
        IUpdateHandler<InterviewSummary, InterviewRejected>,
        IUpdateHandler<InterviewSummary, InterviewRejectedByHQ>,
        IUpdateHandler<InterviewSummary, AnswerCommented>,
        IUpdateHandler<InterviewSummary, UnapprovedByHeadquarters>
    {
        private readonly IUserViewFactory userStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly string unknown = "Unknown";

        public InterviewExportedCommentariesDenormalizer(
            IUserViewFactory userStorage,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.userStorage = userStorage;
            this.questionnaireStorage = questionnaireStorage;
        }

        public InterviewSummary Update(InterviewSummary summary, IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            if (!string.IsNullOrWhiteSpace(evnt.Payload.Comment))
                this.AddInterviewComment(
                    interviewCommentaries: summary,
                    originatorId: evnt.Payload.UserId,
                    comment: evnt.Payload.Comment,
                    roster: String.Empty,
                    variableName: "@@" + InterviewExportedAction.ApprovedByHeadquarter,
                    rosterVector: new decimal[0],
                    timestamp: evnt.EventTimeStamp);
            return summary;
        }

        public InterviewSummary Update(InterviewSummary summary, IPublishedEvent<InterviewCompleted> evnt)
        {
            this.StoreCommentForStatusChange(summary, evnt.Payload.UserId,
             InterviewExportedAction.Completed, evnt.Payload.Comment,
             evnt.Payload.CompleteTime ?? evnt.EventTimeStamp);
            return summary;
        }

        public InterviewSummary Update(InterviewSummary summary, IPublishedEvent<InterviewRestarted> evnt)
        {
            this.StoreCommentForStatusChange(summary, evnt.Payload.UserId,
             InterviewExportedAction.Restarted, evnt.Payload.Comment,
             evnt.Payload.RestartTime ?? evnt.EventTimeStamp);
            return summary;
        }

        public InterviewSummary Update(InterviewSummary summary, IPublishedEvent<InterviewApproved> evnt)
        {
            this.StoreCommentForStatusChange(summary, evnt.Payload.UserId,
             InterviewExportedAction.ApprovedBySupervisor, evnt.Payload.Comment,
             evnt.Payload.ApproveTime ?? evnt.EventTimeStamp);
            return summary;
        }

        public InterviewSummary Update(InterviewSummary summary, IPublishedEvent<InterviewRejected> evnt)
        {
            this.StoreCommentForStatusChange(summary, evnt.Payload.UserId,
                InterviewExportedAction.RejectedBySupervisor, evnt.Payload.Comment,
                evnt.Payload.RejectTime ?? evnt.EventTimeStamp);
            return summary;
        }

        public InterviewSummary Update(InterviewSummary summary, IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            this.StoreCommentForStatusChange(summary, evnt.Payload.UserId,
                InterviewExportedAction.RejectedByHeadquarter, evnt.Payload.Comment, evnt.EventTimeStamp);
            return summary;
        }

        public InterviewSummary Update(InterviewSummary summary, IPublishedEvent<UnapprovedByHeadquarters> evnt)
        {
            this.StoreCommentForStatusChange(summary, evnt.Payload.UserId,
                InterviewExportedAction.UnapprovedByHeadquarter, evnt.Payload.Comment, evnt.EventTimeStamp);
            return summary;
        }

        public InterviewSummary Update(InterviewSummary summary, IPublishedEvent<AnswerCommented> evnt)
        {
            var questionnaire =
                this.questionnaireStorage.GetQuestionnaire(new QuestionnaireIdentity(summary.QuestionnaireId, summary.QuestionnaireVersion), null);

            string roster = string.Empty;

            if (evnt.Payload.RosterVector.Length > 0)
            {
                var lastRosterId = questionnaire.GetRostersFromTopToSpecifiedEntity(evnt.Payload.QuestionId).Last();
                roster = questionnaire.GetRosterVariableName(lastRosterId) ?? unknown;
            }
            
            string variable = questionnaire.GetQuestionVariableName(evnt.Payload.QuestionId);

            this.AddInterviewComment(
             interviewCommentaries: summary,
             originatorId: evnt.Payload.UserId,
             comment: evnt.Payload.Comment,
             roster: roster,
             variableName: variable,
             rosterVector:evnt.Payload.RosterVector,
             timestamp: evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.CommentTime.Value);

            return summary;
        }

        private void StoreCommentForStatusChange(
            InterviewSummary summary,
            Guid statusChangeOriginatorId, 
            InterviewExportedAction statusChangeActionName, 
            string comment, 
            DateTime timestamp)
        {
            if(string.IsNullOrEmpty(comment))
                return; 

            this.AddInterviewComment(
                interviewCommentaries: summary,
                originatorId: statusChangeOriginatorId, 
                comment:comment,
                roster: String.Empty,
                variableName:"@@" + statusChangeActionName,
                rosterVector: new decimal[0], 
                timestamp: timestamp);
        }

        private void AddInterviewComment(
            InterviewSummary interviewCommentaries, 
            Guid originatorId, 
            string comment, 
            string roster, 
            string variableName, 
            decimal[] rosterVector,
            DateTime timestamp)
        {
            var responsible = this.userStorage.GetUser(originatorId);
            var originatorName = responsible == null ? this.unknown : responsible.UserName;
            var originatorRole = responsible == null || !responsible.Roles.Any()
                ? 0
                : responsible.Roles.FirstOrDefault();

            var interviewComment = new InterviewComment(interviewCommentaries)
            {
                Comment = comment,
                CommentSequence = interviewCommentaries.Comments.Count + 1,
                OriginatorUserId = originatorId,
                OriginatorName = originatorName,
                OriginatorRole = originatorRole,
                Roster = roster,
                RosterVector = rosterVector,
                Timestamp = timestamp,
                Variable = variableName
            };
            interviewCommentaries.Comments.Add(interviewComment);
        }
    }
}
