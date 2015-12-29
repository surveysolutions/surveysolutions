using System;
using System.Collections.Generic;
using System.IO;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Controllers
{
    public class InterviewImportStatus
    {
        public InterviewImportStatus()
        {
            State = new InterviewImportState { Errors = new List<InterviewImportError>() };
        }

        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string QuestionnaireTitle { get; set; }
        public bool IsInProgress { get; set; } = false;
        public DateTime StartedDateTime { get; set; }
        public int TotalInterviewsCount { get; set; }
        public int CreatedInterviewsCount { get; set; }
        public double TimePerInterview { get; set; }
        public double ElapsedTime { get; set; }
        public double EstimatedTime { get; set; }
        public InterviewImportState State { get; set; }
    }

    public class InterviewImportState
    {
        public string Delimiter { get; private set; } = "\t";
        public string[] Columns { get; set; }
        public List<InterviewImportError> Errors { get; set; }
    }

    public class InterviewImportError
    {
        public string[] RawData { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class InterviewImportFileDescription
    {
        public bool HasSupervisorColumn { get; set; }
        public bool HasInterviewerColumn { get; set; }
        public List<InterviewImportColumn> ColumnsByPrefilledQuestions { get; set; }
        public byte[] FileBytes { get; set; }
        public List<InterviewImportPrefilledQuestion> PrefilledQuestions { get; set; }
        public string QuestionnaireTitle { get; set; }
        public string[] FileColumns { get; set; }
    }

    public class InterviewImportPrefilledQuestion
    {
        public Guid QuestionId { get; set; }
        public string Variable { get; set; }
        public bool IsRosterSize { get; set; }
        public bool IsGps { get; set; }
        public Type AnswerType { get; set; }
    }

    public class InterviewImportColumn
    {
        public string ColumnName { get; set; }
        public Type ColumnType { get; set; }
        public bool ExistsInFIle { get; set; }
        public bool IsRequired { get; set; }
    }
}