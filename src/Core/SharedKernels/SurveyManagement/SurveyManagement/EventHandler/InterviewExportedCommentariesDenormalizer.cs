using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
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
        private readonly IReadSideRepositoryWriter<UserDocument> userStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureRepository;
        private readonly string unknown = "Unknown";

        public InterviewExportedCommentariesDenormalizer(IReadSideRepositoryWriter<InterviewCommentaries> interviewCommentariesStorage, 
            IReadSideRepositoryWriter<UserDocument> userStorage,
            IPlainKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureRepository)
        {
            this.interviewCommentariesStorage = interviewCommentariesStorage;
            this.userStorage = userStorage;
            this.questionnaireExportStructureRepository = questionnaireExportStructureRepository;
        }

        public override object[] Writers => new[] {this.interviewCommentariesStorage};

        public override object[] Readers
        {
            get { return new object[] { this.userStorage }; }
        }

        public void CleanWritersByEventSource(Guid eventSourceId)
        {
            interviewCommentariesStorage.Remove(eventSourceId);
        }


        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            interviewCommentariesStorage.Store(
                new InterviewCommentaries()
                {
                    InterviewId = evnt.EventSourceId.FormatGuid(),
                    QuestionnaireId = evnt.Payload.QuestionnaireId.FormatGuid(),
                    QuestionnaireVersion = evnt.Payload.QuestionnaireVersion
                }, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            interviewCommentariesStorage.Store(
                new InterviewCommentaries()
                {
                    InterviewId = evnt.EventSourceId.FormatGuid(),
                    QuestionnaireId = evnt.Payload.QuestionnaireId.FormatGuid(),
                    QuestionnaireVersion = evnt.Payload.QuestionnaireVersion
                }, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewRestored> evnt)
        {
            InterviewCommentaries interviewCommentaries = interviewCommentariesStorage.GetById(evnt.EventSourceId);
            if (interviewCommentaries == null)
                return;

            interviewCommentaries.IsDeleted = false;

            interviewCommentariesStorage.Store(interviewCommentaries, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            interviewCommentariesStorage.Store(
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
            InterviewCommentaries interviewCommentaries = interviewCommentariesStorage.GetById(evnt.EventSourceId);
            if (interviewCommentaries == null)
                return;

            interviewCommentaries.IsDeleted = true;

            interviewCommentariesStorage.Store(interviewCommentaries, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            interviewCommentariesStorage.Remove(evnt.EventSourceId);
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
            InterviewCommentaries interviewCommentaries = interviewCommentariesStorage.GetById(evnt.EventSourceId);
            if (interviewCommentaries == null)
                return;

            QuestionnaireExportStructure questionnaire =
                this.questionnaireExportStructureRepository.GetById(new QuestionnaireIdentity(Guid.Parse(interviewCommentaries.QuestionnaireId),interviewCommentaries.QuestionnaireVersion).ToString());
            if (questionnaire == null)
                return;

            string roster = unknown;
            string variable = unknown;

            foreach (var rosterLevel in questionnaire.HeaderToLevelMap.Values)
            {
                ExportedHeaderItem question =
                    rosterLevel.HeaderItems.Values.FirstOrDefault(q => q.PublicKey == evnt.Payload.QuestionId);

                if (question != null)
                {
                    roster = evnt.Payload.RosterVector.Length > 0 ? rosterLevel.LevelName : String.Empty;
                    variable = question.VariableName;
                    break;
                }
            }

            AddInterviewComment(
             interviewCommentaries: interviewCommentaries,
             originatorId: evnt.Payload.UserId,
             comment: evnt.Payload.Comment,
             roster: roster,
             variableName: variable,
             rosterVector:evnt.Payload.RosterVector,
             timestamp: evnt.Payload.CommentTime);

            interviewCommentariesStorage.Store(interviewCommentaries, evnt.EventSourceId);
        }

        private void StoreCommentForStatusChange(Guid interviewId, Guid statusChangeOriginatorId, InterviewExportedAction statusChangeActionName, string comment, DateTime timestamp)
        {
            if(string.IsNullOrEmpty(comment))
                return;

            InterviewCommentaries interviewCommentaries = this.interviewCommentariesStorage.GetById(interviewId);
            if (interviewCommentaries == null)
                return;

            AddInterviewComment(
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
            UserDocument responsible = this.userStorage.GetById(originatorId);
            var originatorName = responsible == null ? this.unknown : responsible.UserName;
            var originatorRole = responsible == null || !responsible.Roles.Any()
                ? UserRoles.Undefined
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