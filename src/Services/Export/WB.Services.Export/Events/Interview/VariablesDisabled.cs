﻿using WB.Services.Export.Events.Interview.Base;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events.Interview
{
    public class VariablesDisabled: InterviewPassiveEvent
    {
        public Identity[] Variables { get; set; }

    }
}
