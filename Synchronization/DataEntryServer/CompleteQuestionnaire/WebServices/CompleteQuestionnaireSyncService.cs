using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;
using SynchronizationMessages.CompleteQuestionnaire;

namespace DataEntryServer.CompleteQuestionnaire.WebServices
{
    public class CompleteQuestionnaireSyncService : WcfService<CompleteQuestionnaireMessage, ErrorCodes>
    {
    }
}
