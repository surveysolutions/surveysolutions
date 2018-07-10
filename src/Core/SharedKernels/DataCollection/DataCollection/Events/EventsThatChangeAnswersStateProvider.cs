using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events
{
    public static class EventsThatChangeAnswersStateProvider
    {
        private static string[] importantEvents;

        public static string[] GetTypeNames()
        {
            if (importantEvents == null)
            {
                List<string> types = new List<string>();

                var allLoadedTypes = typeof(DataCollectionSharedKernelAssemblyMarker).Assembly.GetTypes();
                foreach (var loadedType in allLoadedTypes)
                {
                    if (typeof(QuestionAnswered).IsAssignableFrom(loadedType))
                    {
                        types.Add(loadedType.Name);
                    }
                }

                types.Add(typeof(AnswersRemoved).Name);

                importantEvents = types.ToArray();
            }

            return importantEvents;
        }
    }
}
