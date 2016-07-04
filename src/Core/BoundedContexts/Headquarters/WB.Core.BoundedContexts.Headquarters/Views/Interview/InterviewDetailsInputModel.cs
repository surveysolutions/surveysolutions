﻿using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewDetailsInputModel
    {
        public Guid CompleteQuestionnaireId { get;  set; }

        public UserLight User { get; set; } 

        public Guid? CurrentGroupPublicKey { get; set; }

        public bool IsReverse { get;  set; }

        public Guid? PropagationKey { get; set; }
    }
}
