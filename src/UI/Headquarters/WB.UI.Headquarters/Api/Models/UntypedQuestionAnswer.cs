﻿using System;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Headquarters.Api.Models
{
    internal class UntypedQuestionAnswer
    {
        public Guid Id { get; set; }
        public object Answer { get; set; }
        public QuestionType Type { get; set; }
        public dynamic Settings { get; set; }
    }
}