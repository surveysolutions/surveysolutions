using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    internal interface IDataExportWriter {
        void AddRecords(InterviewDataExportLevelView items, string filePath);
        void AddActionRecord(InterviewActionExportView action, string filePath);
        void CreateHeader(HeaderStructureForLevel header, string filePath);
        void CreateHeaderForActionFile(string filePath);
        string GetInterviewExportedDataFileName(string levelName);
        string GetInterviewActionFileName();
        string[] GetAllDataFiles(string basePath);
        string[] GetApprovedDataFiles(string basePath);
        void DeleteInterviewRecords(string basePath, Guid interviewId);
    }
}