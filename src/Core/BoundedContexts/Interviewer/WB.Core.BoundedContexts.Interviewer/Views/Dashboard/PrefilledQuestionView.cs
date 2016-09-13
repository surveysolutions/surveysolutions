﻿using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class PrefilledQuestionView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string Answer { get; set; }
        public Guid InterviewId { get; set; }
    }
}