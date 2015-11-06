﻿using System;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public abstract class AbstractQuestionCloned : AbstractQuestionAdded
    {
        public Guid? SourceQuestionnaireId { get; set; }
        public Guid SourceQuestionId { get; set; }
        public int TargetIndex { get; set; }
    }
}
