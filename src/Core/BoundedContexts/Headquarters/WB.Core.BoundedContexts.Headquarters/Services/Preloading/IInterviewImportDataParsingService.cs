﻿using System;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    public interface IInterviewImportDataParsingService
    {
        AssignmentImportData[] GetAssignmentsImportDataForSample(string interviewImportProcessId, QuestionnaireIdentity questionnaireIdentity);
        InterviewImportData[] GetInterviewsImportDataForPanel(string interviewImportProcessId, QuestionnaireIdentity questionnaireIdentity);
    }

    public class InterviewImportData
    {
        public Guid? SupervisorId { get; set; }
        public Guid? InterviewerId { get; set; }
        public PreloadedDataDto PreloadedData { get; set; }
    }

    public class AssignmentImportData : InterviewImportData
    {
        public int? Quantity { get; set; }
    }
}