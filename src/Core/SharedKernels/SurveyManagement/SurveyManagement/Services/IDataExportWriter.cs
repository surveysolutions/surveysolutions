using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    internal interface IDataExportWriter {
        void AddActionRecord(InterviewActionExportView actions, string basePath);
        void AddOrUpdateInterviewRecords(InterviewDataExportView items, string basePath);
        void CreateStructure(QuestionnaireExportStructure header, string basePath);
        void DeleteInterviewRecords(string basePath, Guid interviewId);
        void BatchInsert(string basePath, IEnumerable<InterviewDataExportView> interviewDatas, IEnumerable<InterviewActionExportView> interviewActions);
    }
}