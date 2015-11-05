using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class ReadSideRepositoryDataExportWriter : IDataExportWriter
    {
        private readonly IReadSideRepositoryWriter<InterviewExportedDataRecord> interviewExportedDataStorage;
        private readonly ISerializer serializer;

        public ReadSideRepositoryDataExportWriter(
            IReadSideRepositoryWriter<InterviewExportedDataRecord> interviewExportedDataStorage, 
            ISerializer serializer)
        {
            this.interviewExportedDataStorage = interviewExportedDataStorage;
            this.serializer = serializer;
        }

        public void AddOrUpdateInterviewRecords(InterviewDataExportView item, Guid questionnaireId, long questionnaireVersion)
        {
            var interviewExportedData = CreateInterviewExportedData(item, questionnaireId, questionnaireVersion);

            interviewExportedDataStorage.Store(interviewExportedData, item.InterviewId);
        }

        public void DeleteInterviewRecords(Guid interviewId)
        {
            interviewExportedDataStorage.Remove(interviewId);
        }

        public InterviewExportedDataRecord CreateInterviewExportedData(InterviewDataExportView interviewDataExportView, Guid questionnaireId, long questionnaireVersion)
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

                    parametersToConcatenate.AddRange(interviewDataExportRecord.SystemVariableValues);
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
                Data = this.serializer.SerializeToByteArray(interviewData),
            };

            return interviewExportedData;
        }

        public void Clear()
        {
            ((IReadSideRepositoryCleaner)interviewExportedDataStorage).Clear();
        }

        public Type ViewType
        {
            get { return GetType(); }
        }

        public string GetReadableStatus()
        {
            return ((ICacheableRepositoryWriter)interviewExportedDataStorage).GetReadableStatus();
        }

        public void EnableCache()
        {
            ((ICacheableRepositoryWriter)interviewExportedDataStorage).EnableCache();
        }

        public void DisableCache()
        {
            ((ICacheableRepositoryWriter)interviewExportedDataStorage).DisableCache();
        }

        public bool IsCacheEnabled
        {
            get { return ((ICacheableRepositoryWriter)interviewExportedDataStorage).IsCacheEnabled; }
        }
    }
}
