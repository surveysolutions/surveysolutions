using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class InterviewExportedDataDenormalizer : IEventHandler<InterviewApprovedByHQ>, 
        IEventHandler<SupervisorAssigned>,
        IEventHandler<InterviewerAssigned>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<InterviewApproved>,
        IEventHandler<InterviewRejected>,
        IEventHandler<InterviewRejectedByHQ>,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler< MultipleOptionsQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
        IEventHandler<NumericQuestionAnswered>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>,
        IEventHandler<TextListQuestionAnswered>,
        IEventHandler<QRBarcodeQuestionAnswered>,

        IEventHandler
    {
        private const string UnknownUserRole = "<UNKNOWN ROLE>";
        private readonly IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> questionnaireExportStructureWriter;
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IDataExportService dataExportService;

        private readonly InterviewExportedAction[] listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded = new[]
        { InterviewExportedAction.InterviewerAssigned, InterviewExportedAction.RejectedBySupervisor, InterviewExportedAction.Restarted };

        public InterviewExportedDataDenormalizer(IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter,
            IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            IDataExportService dataExportService, IReadSideRepositoryWriter<UserDocument> users)
        {
            this.interviewDataWriter = interviewDataWriter;
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.dataExportService = dataExportService;
            this.users = users;
        }

        public string Name { get { return this.GetType().Name; } }

        public Type[] UsesViews
        {
            get { return new Type[] { typeof(InterviewData), typeof(UserDocument), typeof(InterviewActionLog), typeof(QuestionnaireExportStructure) }; }
        }

        public Type[] BuildsViews { get { return new Type[] { typeof(InterviewDataExportView), typeof(InterviewActionLog) }; } }

        public void Handle(IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.EventTimeStamp, InterviewExportedAction.ApproveByHeadquarter);
        }

        private void UpdateInterviewData(Guid interviewId, Guid userId, DateTime timeStamp, InterviewExportedAction action)
        {
            var interview = this.interviewDataWriter.GetById(interviewId);

            var exportStructure = this.questionnaireExportStructureWriter.GetById(interview.Document.QuestionnaireId,
                interview.Document.QuestionnaireVersion);

            var interviewDataExportView = new InterviewDataExportView(interviewId, interview.Document.QuestionnaireId,
                interview.Document.QuestionnaireVersion,
                exportStructure.HeaderToLevelMap.Values.Select(
                    exportStructureForLevel =>
                        new InterviewDataExportLevelView(exportStructureForLevel.LevelScopeVector, exportStructureForLevel.LevelName,
                            this.BuildRecordsForHeader(interview.Document, exportStructureForLevel), interviewId.FormatGuid())).ToArray());

            this.dataExportService.AddExportedDataByInterview(interviewDataExportView);

            UserDocument responsible = this.users.GetById(userId);
            var userName = this.GetUserName(responsible);

            this.dataExportService.AddInterviewAction(interview.Document.QuestionnaireId,
                interview.Document.QuestionnaireVersion, CreateInterviewActionExportView(interviewId, action,
                    userName, this.GetUserRole(responsible), timeStamp));
        }
        private InterviewDataExportRecord[] BuildRecordsForHeader(InterviewData interview, HeaderStructureForLevel headerStructureForLevel)
        {
            var dataRecords = new List<InterviewDataExportRecord>();

            var interviewDataByLevels = this.GetLevelsFromInterview(interview, headerStructureForLevel.LevelScopeVector);
          
            foreach (var dataByLevel in interviewDataByLevels)
            {
                var vectorLength = dataByLevel.RosterVector.Length;
                
                string recordId = vectorLength == 0 ?
                    interview.InterviewId.FormatGuid() :
                    dataByLevel.RosterVector.Last().ToString(CultureInfo.InvariantCulture);

                var parentRecordIds = new string[dataByLevel.RosterVector.Length];
                if (parentRecordIds.Length > 0)
                {
                    parentRecordIds[0] = interview.InterviewId.FormatGuid();
                    for (int i = 0; i < dataByLevel.RosterVector.Length-1; i++)
                    {
                        parentRecordIds[i+1] = dataByLevel.RosterVector[i].ToString(CultureInfo.InvariantCulture);
                    }

                    parentRecordIds = parentRecordIds.Reverse().ToArray();
                }

                string[] referenceValues = new string[0];

                if (headerStructureForLevel.IsTextListScope)
                {
                    referenceValues = new string[]{ GetTextValueForTextListQuestion(interview, dataByLevel.RosterVector, headerStructureForLevel.LevelScopeVector.Last())};
                }

                dataRecords.Add(new InterviewDataExportRecord(interview.InterviewId, recordId, referenceValues, parentRecordIds,
                    this.GetQuestionsForExport(dataByLevel.GetAllQuestions(), headerStructureForLevel)));
            }

            return dataRecords.ToArray();
        }

        private string CreateLevelIdFromPropagationVector(decimal[] vector)
        {
            return vector.Length == 0 ? "#" : EventHandlerUtils.CreateLeveKeyFromPropagationVector(vector);
        }

        private string GetTextValueForTextListQuestion(InterviewData interview, decimal[] rosterVector, Guid id)
        {
            decimal itemToSearch = rosterVector.Last();

            for(var i = 1; i <= rosterVector.Length; i++)
            {
                var levelForVector =
                    interview.Levels.SingleOrDefault(
                        l => l.Key == CreateLevelIdFromPropagationVector(rosterVector.Take(rosterVector.Length - i).ToArray()));

                var questionToCheck = levelForVector.Value.GetQuestion(id);

                if (questionToCheck == null) 
                    continue;
                if (questionToCheck.Answer == null) 
                    return string.Empty;
                var interviewTextListAnswer = questionToCheck.Answer as InterviewTextListAnswers;

                if (interviewTextListAnswer == null) 
                    return string.Empty;
                var item = interviewTextListAnswer.Answers.SingleOrDefault(a => a.Value == itemToSearch);
                
                return item != null ? item.Answer : string.Empty;
            }

            return string.Empty;
        }

        private ExportedQuestion[] GetQuestionsForExport(IEnumerable<InterviewQuestion> availableQuestions, HeaderStructureForLevel headerStructureForLevel)
        {
            var result = new List<ExportedQuestion>();
            foreach (var headerItem in headerStructureForLevel.HeaderItems.Values)
            {
                var question = availableQuestions.FirstOrDefault(q => q.Id == headerItem.PublicKey);
                ExportedQuestion exportedQuestion = null;
                if (question == null)
                {
                    var ansnwers = new List<string>();
                    for (int i = 0; i < headerItem.ColumnNames.Count(); i++)
                    {
                        ansnwers.Add(string.Empty);
                    }
                    exportedQuestion = new ExportedQuestion(headerItem.PublicKey, headerItem.QuestionType, ansnwers.ToArray());
                }
                else
                {
                    exportedQuestion = new ExportedQuestion(question, headerItem);
                }

                result.Add(exportedQuestion);
            }
            return result.ToArray();
        }

        private IEnumerable<InterviewLevel> GetLevelsFromInterview(InterviewData interview, ValueVector<Guid> levelVector)
        {
            if (!levelVector.Any())
                return interview.Levels.Values.Where(level => level.ScopeVectors.ContainsKey(new ValueVector<Guid>()));
            return interview.Levels.Values.Where(level => level.ScopeVectors.ContainsKey(levelVector));
        }

        private string GetUserName(UserDocument responsible)
        {
            var userName = responsible != null ? responsible.UserName : "<UNKNOWN USER>";
            return userName;
        }

        private void RecordFirstAnswerIfNeeded(Guid interviewId, Guid userId, DateTime answerTime)
        {
            //TODO record first answer

      /*      var interviewActionLog = this.interviewActionLogs.GetById(interviewId);
            if (interviewActionLog == null)
                return;

            if (!interviewActionLog.Actions.Any() || !listOfActionsAfterWhichFirstAnswerSetAtionShouldBeRecorded.Contains(interviewActionLog.Actions.Last().Action))
                return;

            UserDocument responsible = this.users.GetById(userId);
            if (responsible == null || !responsible.Roles.Contains(UserRoles.Operator))
                return;

            interviewActionLog.Actions.Add(CreateInterviewActionExportView(interviewId, InterviewExportedAction.FirstAnswerSet,
                responsible.UserName, this.GetUserRole(responsible), answerTime));

            interviewActionLogs.Store(interviewActionLog, interviewId);*/
        }

        private string GetUserRole(UserDocument user)
        {
            if (user==null || !user.Roles.Any())
                return UnknownUserRole;
            var firstRole = user.Roles.First();
            switch (firstRole)
            {
                case UserRoles.Operator: return "Interviewer";
                case UserRoles.Supervisor: return "Supervisor";
                case UserRoles.Headquarter: return "Headquarter";
            }
            return UnknownUserRole;
        }

        private InterviewActionExportView CreateInterviewActionExportView(Guid interviewId, InterviewExportedAction action, string userName, string userRole,
            DateTime timestamp)
        {
            return new InterviewActionExportView(interviewId.FormatGuid(), action, userName, timestamp, userRole);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.EventTimeStamp, InterviewExportedAction.SupervisorAssigned);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.EventTimeStamp, InterviewExportedAction.InterviewerAssigned);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.CompleteTime ?? evnt.EventTimeStamp, InterviewExportedAction.Completed);
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.RestartTime ?? evnt.EventTimeStamp, InterviewExportedAction.Restarted);
        }

        public void Handle(IPublishedEvent<InterviewApproved> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.EventTimeStamp, InterviewExportedAction.ApproveBySupervisor);
        }

        public void Handle(IPublishedEvent<InterviewRejected> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.EventTimeStamp, InterviewExportedAction.RejectedBySupervisor);
        }

        public void Handle(IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            UpdateInterviewData(evnt.EventSourceId, evnt.Payload.UserId, evnt.EventTimeStamp, InterviewExportedAction.RejectedByHeadquarter);
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<NumericQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<TextListQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            this.RecordFirstAnswerIfNeeded(evnt.EventSourceId, evnt.Payload.UserId, evnt.Payload.AnswerTime);
        }
    }
}
