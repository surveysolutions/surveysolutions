using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events
{
    public static class EventsThatAssignInterviewToResponsibleProvider
    {
        private static string[] eventNames;

        public static string[] GetInterviewerOrSupervisorTypeNames()
        {
            if (eventNames == null)
            {
                List<string> types = new List<string>();

                types.Add(typeof(InterviewerAssigned).Name);
                types.Add(typeof(SupervisorAssigned).Name);
                types.Add(typeof(InterviewRejected).Name);
                types.Add(typeof(InterviewRejectedByHQ).Name);

                eventNames = types.ToArray();
            }

            return eventNames;
        }
    }
}
