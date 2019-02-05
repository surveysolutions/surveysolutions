using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class VariablesEnabled : InterviewPassiveEvent
    {
        public Identity[] Variables { get; private set; }

    }
}
