using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
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
      /*  IEventHandler<SupervisorAssigned>,
        IEventHandler<InterviewerAssigned>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<InterviewApproved>,
        IEventHandler<InterviewRejected>,
        IEventHandler<InterviewRejectedByHQ>,*/
        IEventHandler
    {
        private readonly IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> questionnaireExportStructureWriter;
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummares;
        private readonly IDataExportService dataExportService;

        public InterviewExportedDataDenormalizer(IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviewDataWriter,
            IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure> questionnaireExportStructureWriter,
            IDataExportService dataExportService, IReadSideRepositoryWriter<UserDocument> users, IReadSideRepositoryWriter<InterviewSummary> interviewSummares)
        {
            this.interviewDataWriter = interviewDataWriter;
            this.questionnaireExportStructureWriter = questionnaireExportStructureWriter;
            this.dataExportService = dataExportService;
            this.users = users;
            this.interviewSummares = interviewSummares;
        }

        public string Name { get { return this.GetType().Name; } }

        public Type[] UsesViews
        {
            get { return new Type[] { typeof(InterviewData), typeof(QuestionnaireExportStructure) }; }
        }

        public Type[] BuildsViews { get { return new Type[] { typeof(InterviewDataExportView) }; } }

        public void Handle(IPublishedEvent<InterviewApprovedByHQ> evnt)
        {
            var interview = this.interviewDataWriter.GetById(evnt.EventSourceId);
            var interviewSummary = this.interviewSummares.GetById(evnt.EventSourceId);

            var exportStructure = this.questionnaireExportStructureWriter.GetById(interview.Document.QuestionnaireId,
                interview.Document.QuestionnaireVersion);

            var interviewDataExportView = new InterviewDataExportView(interview.Document.QuestionnaireId, interview.Document.QuestionnaireVersion,
                exportStructure.HeaderToLevelMap.Values.Select(
                    exportStructureForLevel => 
                        new InterviewDataExportLevelView(exportStructureForLevel.LevelScopeVector, exportStructureForLevel.LevelName, 
                            this.BuildRecordsForHeader(interview.Document, exportStructureForLevel))).ToArray());
            
            this.dataExportService.AddExportedDataByInterview(interviewDataExportView);

            StoreInterviewActionForUser(interview.Document.QuestionnaireId,
                interview.Document.QuestionnaireVersion, evnt.EventSourceId, evnt.Payload.UserId,
                interviewSummary == null ? new List<InterviewCommentedStatus>() : interviewSummary.CommentedStatusesHistory,
                evnt.EventTimeStamp);
        }

        private void StoreInterviewActionForUser(Guid questionnaireId, long questionnaireVersion, Guid interviewId, Guid userId, List<InterviewCommentedStatus> commentedStatusesHistory, DateTime timestamptimestamp)
        {
            UserDocument responsible = this.users.GetById(userId);
            var userName = responsible != null ? responsible.UserName : "<UNKNOWN USER>";
            var isApprovedByHeadquarterRecorded = false;
            foreach (var interviewCommentedStatus in commentedStatusesHistory)
            {
                if (interviewCommentedStatus.Status == InterviewStatus.ApprovedByHeadquarters)
                    isApprovedByHeadquarterRecorded = true;

                var action = CreateActionFromStatus(interviewCommentedStatus.Status);
                if (action.HasValue)
                    this.dataExportService.AddInterviewAction(new InterviewActionExportView(questionnaireId,
                        questionnaireVersion, interviewId.FormatGuid(), action.Value, interviewCommentedStatus.Responsible, interviewCommentedStatus.Date));
            }
            if (!isApprovedByHeadquarterRecorded)
                this.dataExportService.AddInterviewAction(new InterviewActionExportView(questionnaireId,
                       questionnaireVersion, interviewId.FormatGuid(), InterviewExportedAction.ApproveByHeadquarter, userName, timestamptimestamp));
        }

        private InterviewExportedAction? CreateActionFromStatus(InterviewStatus status)
        {
            switch (status)
            {
                case InterviewStatus.SupervisorAssigned:
                    return InterviewExportedAction.SupervisorAssigned;
                case InterviewStatus.InterviewerAssigned:
                    return InterviewExportedAction.InterviewerAssigned;
                case InterviewStatus.Completed:
                    return InterviewExportedAction.Completed;
                case InterviewStatus.Restarted:
                    return InterviewExportedAction.Restarted;
                case InterviewStatus.ApprovedBySupervisor:
                    return InterviewExportedAction.ApproveBySupervisor;
                case InterviewStatus.ApprovedByHeadquarters:
                    return InterviewExportedAction.ApproveByHeadquarter;
                case InterviewStatus.RejectedBySupervisor:
                    return InterviewExportedAction.RejectedBySupervisor;
                case InterviewStatus.RejectedByHeadquarters:
                    return InterviewExportedAction.RejectedByHeadquarter;
            }
            return null;
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
                    exportedQuestion = new ExportedQuestion(headerItem.PublicKey, ansnwers.ToArray());
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

    /*    public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            this.StoreInterviewActionForUser(evnt.EventSourceId, evnt.Payload.UserId, InterviewExportedAction.SupervisorAssigned,
              evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            this.StoreInterviewActionForUser(evnt.EventSourceId, evnt.Payload.UserId, InterviewExportedAction.InterviewerAssigned,
                evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            this.StoreInterviewActionForUser(evnt.EventSourceId, evnt.Payload.UserId, InterviewExportedAction.Completed,
              evnt.Payload.CompleteTime);
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            this.StoreInterviewActionForUser(evnt.EventSourceId, evnt.Payload.UserId, InterviewExportedAction.Restarted,
                evnt.Payload.RestartTime);
        }

        public void Handle(IPublishedEvent<InterviewApproved> evnt)
        {
            this.StoreInterviewActionForUser(evnt.EventSourceId, evnt.Payload.UserId, InterviewExportedAction.ApproveBySupervisor,
               evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewRejected> evnt)
        {
            this.StoreInterviewActionForUser(evnt.EventSourceId, evnt.Payload.UserId, InterviewExportedAction.RejectedBySupervisor,
               evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<InterviewRejectedByHQ> evnt)
        {
            this.StoreInterviewActionForUser(evnt.EventSourceId, evnt.Payload.UserId, InterviewExportedAction.RejectedByHeadquarter,
                evnt.EventTimeStamp);
        }*/
    }
}
