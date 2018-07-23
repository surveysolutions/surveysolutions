using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewExportedCommentariesDenormalizer:
        BaseDenormalizer, IAtomicEventHandler,
        IEventHandler<InterviewOnClientCreated>,
        IEventHandler<InterviewFromPreloadedDataCreated>,
        IEventHandler<InterviewRestored>,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewApprovedByHQ>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<InterviewApproved>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewHardDeleted>,
        IEventHandler<InterviewRejected>,
        IEventHandler<InterviewRejectedByHQ>,
        IEventHandler<AnswerCommented>,
        IEventHandler<UnapprovedByHeadquarters>
    {
        private readonly IReadSideRepositoryWriter<InterviewCommentaries> interviewCommentariesStorage;
        private readonly IUserViewFactory userStorage;
        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly string unknown = "Unknown";

        public InterviewExportedCommentariesDenormalizer(
            IReadSideRepositoryWriter<InterviewCommentaries> interviewCommentariesStorage,
            IUserViewFactory userStorage,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage)
        {
            this.interviewCommentariesStorage = interviewCommentariesStorage;
            this.userStorage = userStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        public override object[] Writers => new[] {this.interviewCommentariesStorage};

        public override object[] Readers => new object[0];

        public void CleanWritersByEventSource(Guid eventSourceId)
        {
            this.interviewCommentariesStorage.Remove(eventSourceId);
        }


        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            this.interviewCommentariesStorage.Store(
                new InterviewCommentaries()
                {
                    InterviewId = evnt.EventSourceId.FormatGuid(),
                    QuestionnaireId = evnt.Payload.QuestionnaireId.FormatGuid(),
                    QuestionnaireVersion = evnt.Payload.QuestionnaireVersion
                }, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            this.interviewCommentariesStorage.Store(
                new InterviewCommentaries()
                {
                    InterviewId = evnt.EventSourceId.FormatGuid(),
                    QuestionnaireId = evnt.Payload.QuestionnaireId.FormatGuid(),
                    QuestionnaireVersion = evnt.Payload.QuestionnaireVersion
                }, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewRestored> evnt)
        {
            InterviewCommentaries interviewCommentaries = this.interviewCommentariesStorage.GetById(evnt.EventSourceId);
            if (interviewCommentaries == null)
                return;

            interviewCommentaries.IsDeleted = false;

            this.interviewCommentariesStorage.Store(interviewCommentaries, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            this.interviewCommentariesStorage.Store(
                new InterviewCommentaries()
                {
                    InterviewId = evnt.EventSourceId.FormatGuid(),
                    QuestionnaireId = evnt.Payload.QuestionnaireId.FormatGuid(),
                    QuestionnaireVersion = evnt.Payload.QuestionnaireVersion
                }, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            Guid interviewId = evnt.EventSourceId;
            InterviewCommentaries interviewCommentaries = this.interviewCommentariesStorage.GetById(interviewId);
            if (interviewCommentaries == null)
                return;

            if (!string.IsNullOrEmpty(evnt.Payload.Comment))
                this.AddInterviewComment(
                    interviewCommentaries: interviewCommentaries,
                    originatorId: evnt.Payload.UserId,
                    comment: evnt.Payload.Comment,
                    roster: String.Empty,
                    variableName: "@@" + InterviewExportedAction.ApprovedByHeadquarter,
                    rosterVector: new decimal[0],
                    timestamp: evnt.EventTimeStamp);

            interviewCommentaries.IsApprovedByHQ = true;

            this.interviewCommentariesStorage.Store(interviewCommentaries, interviewId);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            this.StoreCommentForStatusChange(evnt.EventSourceId, evnt.Payload.UserId,
             InterviewExportedAction.Completed, evnt.Payload.Comment,
             evnt.Payload.CompleteTime ?? evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            this.StoreCommentForStatusChange(evnt.EventSourceId, evnt.Payload.UserId,
             InterviewExportedAction.Restarted, evnt.Payload.Comment,
             evnt.Payload.RestartTime ?? evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewApproved> evnt)
        {
            this.StoreCommentForStatusChange(evnt.EventSourceId, evnt.Payload.UserId,
             InterviewExportedAction.ApprovedBySupervisor, evnt.Payload.Comment,
             evnt.Payload.ApproveTime ?? evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            InterviewCommentaries interviewCommentaries = this.interviewCommentariesStorage.GetById(evnt.EventSourceId);
            if (interviewCommentaries == null)
                return;

            interviewCommentaries.IsDeleted = true;

            this.interviewCommentariesStorage.Store(interviewCommentaries, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            this.interviewCommentariesStorage.Remove(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewRejected> evnt)
        {
            this.StoreCommentForStatusChange(evnt.EventSourceId, evnt.Payload.UserId,
                InterviewExportedAction.RejectedBySupervisor, evnt.Payload.Comment,
                evnt.Payload.RejectTime ?? evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            this.StoreCommentForStatusChange(evnt.EventSourceId, evnt.Payload.UserId,
                InterviewExportedAction.RejectedByHeadquarter, evnt.Payload.Comment, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<UnapprovedByHeadquarters> evnt)
        {
            this.StoreCommentForStatusChange(evnt.EventSourceId, evnt.Payload.UserId,
                InterviewExportedAction.UnapprovedByHeadquarter, evnt.Payload.Comment, evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<AnswerCommented> evnt)
        {
            InterviewCommentaries interviewCommentaries = this.interviewCommentariesStorage.GetById(evnt.EventSourceId);
            if (interviewCommentaries == null)
                return;

            QuestionnaireExportStructure questionnaire =
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(new QuestionnaireIdentity(Guid.Parse(interviewCommentaries.QuestionnaireId),interviewCommentaries.QuestionnaireVersion));
            if (questionnaire == null)
                return;

            string roster = this.unknown;
            string variable = this.unknown;

            foreach (var rosterLevel in questionnaire.HeaderToLevelMap.Values)
            {
                ExportedQuestionHeaderItem question =
                    rosterLevel.HeaderItems.Values.FirstOrDefault(q => q.PublicKey == evnt.Payload.QuestionId) as ExportedQuestionHeaderItem;

                if (question != null)
                {
                    roster = evnt.Payload.RosterVector.Length > 0 ? rosterLevel.LevelName : String.Empty;
                    variable = question.VariableName;
                    break;
                }
            }

            this.AddInterviewComment(
             interviewCommentaries: interviewCommentaries,
             originatorId: evnt.Payload.UserId,
             comment: evnt.Payload.Comment,
             roster: roster,
             variableName: variable,
             rosterVector:evnt.Payload.RosterVector,
             timestamp: evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.CommentTime.Value);

            this.interviewCommentariesStorage.Store(interviewCommentaries, evnt.EventSourceId);
        }

        private void StoreCommentForStatusChange(Guid interviewId, Guid statusChangeOriginatorId, InterviewExportedAction statusChangeActionName, string comment, DateTime timestamp)
        {
            if(string.IsNullOrEmpty(comment))
                return;

            InterviewCommentaries interviewCommentaries = this.interviewCommentariesStorage.GetById(interviewId);
            if (interviewCommentaries == null)
                return;

            this.AddInterviewComment(
                interviewCommentaries: interviewCommentaries,
                originatorId: statusChangeOriginatorId, 
                comment:comment,
                roster: String.Empty,
                variableName:"@@" + statusChangeActionName,
                rosterVector: new decimal[0], 
                timestamp: timestamp);

            interviewCommentaries.IsApprovedByHQ = InterviewExportedAction.ApprovedByHeadquarter ==
                                                   statusChangeActionName;

            this.interviewCommentariesStorage.Store(interviewCommentaries, interviewId);
        }

        private void AddInterviewComment(
            InterviewCommentaries interviewCommentaries, 
            Guid originatorId, 
            string comment, 
            string roster, 
            string variableName, 
            decimal[] rosterVector,
            DateTime timestamp)
        {
            UserView responsible = this.userStorage.GetUser(new UserViewInputModel(originatorId));
            var originatorName = responsible == null ? this.unknown : responsible.UserName;
            var originatorRole = responsible == null || !responsible.Roles.Any()
                ? 0
                : responsible.Roles.FirstOrDefault();

            var interviewComment = new InterviewComment()
            {
                Comment = comment,
                CommentSequence = interviewCommentaries.Commentaries.Count + 1,
                OriginatorUserId = originatorId,
                OriginatorName = originatorName,
                OriginatorRole = originatorRole,
                Roster = roster,
                RosterVector = rosterVector,
                Timestamp = timestamp,
                Variable = variableName
            };
            interviewCommentaries.Commentaries.Add(interviewComment);
        }
    }
}
