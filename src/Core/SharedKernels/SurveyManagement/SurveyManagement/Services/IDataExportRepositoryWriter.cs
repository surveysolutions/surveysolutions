﻿using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IDataExportRepositoryWriter : IReadSideRepositoryCleaner, IReadSideRepositoryWriter
    {
        string GetFilePathToExportedCompressedData(Guid questionnaireId, long version);
        string GetFilePathToExportedApprovedCompressedData(Guid questionnaireId, long version);
        string GetFilePathToExportedBinaryData(Guid questionnaireId, long version);
        void AddExportedDataByInterview(Guid interviewId);
        void DeleteInterview(Guid interviewId);
        void AddInterviewAction(InterviewExportedAction action, Guid interviewId, Guid userId, DateTime timestamp);
        void CreateExportStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure);
        void DeleteExportedDataForQuestionnaireVersion(Guid questionnaireId, long questionnaireVersion);
    }
}
