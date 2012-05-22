using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using SynchronizationMessages.CompleteQuestionnaire;

namespace DataEntryWCFServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class CompleteQuestionnaireSyncService : ICompleteQuestionnaireService
    {
        public ErrorCodes Process(EventSyncMessage request)
        {
            Console.WriteLine("======================================================================");
          
            return ErrorCodes.None;
        }
    }
}
