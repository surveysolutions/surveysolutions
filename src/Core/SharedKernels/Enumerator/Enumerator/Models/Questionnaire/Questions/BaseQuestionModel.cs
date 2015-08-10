﻿using System;

namespace WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions
{
    public abstract class BaseQuestionModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public bool IsPrefilled { get; set; }
        public string Instructions { get; set; }
        public bool IsMandatory { get; set; }
        public string ValidationMessage { get; set; }
        public string Variable { get; set; }
    }
}