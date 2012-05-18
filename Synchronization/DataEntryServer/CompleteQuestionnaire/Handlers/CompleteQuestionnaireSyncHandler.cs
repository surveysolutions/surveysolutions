using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;
using SynchronizationMessages.CompleteQuestionnaire;

namespace DataEntryServer.CompleteQuestionnaire.Handlers
{
   
    public class CompleteQuestionnaireSyncHandler : IHandleMessages<CompleteQuestionnaireMessage>
    {
        private readonly IBus bus;

        public CompleteQuestionnaireSyncHandler(IBus bus)
        {
            this.bus = bus;
        }

        public void Handle(CompleteQuestionnaireMessage message)
        {
            Console.WriteLine("======================================================================");

          /*  if (message.OrderId % 2 == 0)
                bus.Return((int)ErrorCodes.Fail);
            else*/
                bus.Return((int)ErrorCodes.None);
        }
    }
}
