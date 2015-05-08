using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class ReadSideRepositoryDataExportWriter : IDataExportWriter
    {
        private readonly IReadSideRepositoryWriter<InterviewExportedDataRecord> interviewExportedDataStorage;
        private readonly IReadSideRepositoryWriter<InterviewHistory> interviewActionsDataStorage;
        private readonly IJsonUtils jsonUtils;

        public ReadSideRepositoryDataExportWriter(
            IReadSideRepositoryWriter<InterviewExportedDataRecord> interviewExportedDataStorage, 
            IReadSideRepositoryWriter<InterviewHistory> interviewActionsDataStorage, 
            IJsonUtils jsonUtils)
        {
            this.interviewExportedDataStorage = interviewExportedDataStorage;
            this.interviewActionsDataStorage = interviewActionsDataStorage;
            this.jsonUtils = jsonUtils;
        }

        public void AddActionRecord(InterviewActionExportView action, Guid questionnaireId, long questionnaireVersion)
        {
            var history = interviewActionsDataStorage.GetById(action.InterviewId);
            if (history == null)
            {
                history = new InterviewHistory()
                {
                    InterviewId = action.InterviewId,
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion
                };
            }

            history.InterviewActions.Add(CreateInterviewAction(action));

            interviewActionsDataStorage.Store(history, history.InterviewId);

            var interviewExportedData = interviewExportedDataStorage.GetById(history.InterviewId);
            if (interviewExportedData != null)
            {
                interviewExportedData.LastAction = action.Action;
                interviewExportedDataStorage.Store(interviewExportedData, history.InterviewId);
            }
        }

        public void AddOrUpdateInterviewRecords(InterviewDataExportView item, Guid questionnaireId, long questionnaireVersion)
        {
            var interviewExportedData = CreateInterviewExportedData(item, questionnaireId, questionnaireVersion);

            interviewExportedDataStorage.Store(interviewExportedData, item.InterviewId);
        }

        public void DeleteInterviewRecords(Guid interviewId)
        {
            DeleteInterviewExportedDataByInterviewId(interviewId);
            DeleteInterviewActionsByInterviewId(interviewId.FormatGuid());
        }

        private void DeleteInterviewExportedDataByInterviewId(Guid interviewId)
        {
            interviewExportedDataStorage.Remove(interviewId);
        }

        private void DeleteInterviewActionsByInterviewId(string interviewId)
        {
            interviewActionsDataStorage.Remove(interviewId);
        }

        private InterviewAction CreateInterviewAction(InterviewActionExportView action)
        {
            return new InterviewAction()
            {
                Action = action.Action,
                Originator = action.Originator,
                Role = action.Role,
                Timestamp = action.Timestamp
            };
        }

        private InterviewExportedDataRecord CreateInterviewExportedData(InterviewDataExportView interviewDataExportView, Guid questionnaireId, long questionnaireVersion)
        {
            var interviewData = new Dictionary<string, string[]>();
            
            var stringSeparator = ExportFileSettings.SeparatorOfExportedDataFile.ToString();
            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                var recordsByLevel = new List<string>();
                foreach (var interviewDataExportRecord in interviewDataExportLevelView.Records)
                {
                    var parametersToConcatenate = new List<string> {interviewDataExportRecord.RecordId};
                    
                    parametersToConcatenate.AddRange(interviewDataExportRecord.ReferenceValues);

                    foreach (var exportedQuestion in interviewDataExportRecord.Questions)
                    {
                        parametersToConcatenate.AddRange(exportedQuestion.Answers.Select(itemValue => string.IsNullOrEmpty(itemValue) ? "" : itemValue));
                    }

                    parametersToConcatenate.AddRange(interviewDataExportRecord.ParentRecordIds);

                    recordsByLevel.Add(string.Join(stringSeparator,
                            parametersToConcatenate.Select(v => v.Replace(stringSeparator, ""))));
                }
                interviewData.Add(interviewDataExportLevelView.LevelName, recordsByLevel.ToArray());
            }
            var interviewExportedData = new InterviewExportedDataRecord
            {
                InterviewId = interviewDataExportView.InterviewId.FormatGuid(),
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Data = jsonUtils.SerializeToByteArray(interviewData)
            };

            return interviewExportedData;
        }

        public void Clear()
        {
            ((IReadSideRepositoryCleaner)interviewActionsDataStorage).Clear();
            ((IReadSideRepositoryCleaner)interviewExportedDataStorage).Clear();
        }

        public Type ViewType
        {
            get { return GetType(); }
        }

        public string GetReadableStatus()
        {
            var status1 = ((IChacheableRepositoryWriter) interviewActionsDataStorage).GetReadableStatus();
            var status2 = ((IChacheableRepositoryWriter)interviewExportedDataStorage).GetReadableStatus();
            return status1 + Environment.NewLine + status2;
        }

        public void EnableCache()
        {
            ((IChacheableRepositoryWriter)interviewActionsDataStorage).EnableCache();
            ((IChacheableRepositoryWriter)interviewExportedDataStorage).EnableCache();
        }

        public void DisableCache()
        {
            ((IChacheableRepositoryWriter)interviewActionsDataStorage).DisableCache();
            ((IChacheableRepositoryWriter)interviewExportedDataStorage).DisableCache();
        }

        public bool IsCacheEnabled
        {
            get { return ((IChacheableRepositoryWriter) interviewActionsDataStorage).IsCacheEnabled; }
        }
    }
}
