using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events
{
    public class EventsThatChangeAddedInStreamAfterSynchronization
    {
        private static string[] eventNames;

        public static string[] GetTypeNames()
        {
            if (eventNames == null)
            {
                List<string> types = new List<string>();

                types.Add(typeof(MarkInterviewAsReceivedByInterviewer).Name);

                eventNames = types.ToArray();
            }

            return eventNames;
        }
    }
}
