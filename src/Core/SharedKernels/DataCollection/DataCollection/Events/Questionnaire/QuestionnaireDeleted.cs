﻿using System;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.SharedKernels.DataCollection.Events.Questionnaire
{
    [Obsolete("v5.7")]
    public class QuestionnaireDeleted : IEvent
    {
        public long QuestionnaireVersion { get; set; }
        public Guid? ResponsibleId { get; set; }
    }
}
