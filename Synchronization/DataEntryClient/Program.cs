using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Newtonsoft.Json;
using Raven.Client.Document;
using RavenQuestionnaire.Core.Documents;
using SynchronizationMessages.CompleteQuestionnaire;

namespace DataEntryClient
{
    internal class Program
    {
        private static readonly ChannelFactory<ICompleteQuestionnaireService> ChannelFactory = new ChannelFactory<ICompleteQuestionnaireService>("");

        private static void Main()
        {
            Console.WriteLine("This will send requests to the CancelOrder WCF service");
            Console.WriteLine("Press 'Enter' to send a message.To exit, Ctrl + C");

            ICompleteQuestionnaireService client = ChannelFactory.CreateChannel();
           

            try
            {
                var store = new DocumentStore() {Url = "http://localhost:8080"}.Initialize();
                var session = store.OpenSession();
                var questionnaires = session.Query<CompleteQuestionnaireDocument>();
                foreach (var completeQuestionnaireDocument in questionnaires)
                {
                    
               
                    var message = new CompleteQuestionnaireMessage
                    {
                       SynchronizationKey = Guid.NewGuid(),
                       Questionanire = JsonConvert.SerializeObject(completeQuestionnaireDocument)
                    };

                    Console.WriteLine("Sending message with SynchronizationKey {0}.", message.SynchronizationKey);

                    ErrorCodes returnCode = client.Process(message);

                    Console.WriteLine("Error code returned: " + returnCode);
                }
            }
            finally
            {
                try
                {
                    ((IChannel)client).Close();
                }
                catch
                {
                    ((IChannel)client).Abort();
                }
            }
        }
    }
}
