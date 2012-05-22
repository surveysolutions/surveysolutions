using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Raven.Client.Document;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Documents;
using SynchronizationMessages.CompleteQuestionnaire;

namespace DataEntryClient.CompleteQuestionnaire
{
    public class CompleteQuestionnaireSync
    {
        private ICommandInvoker invoker; 
        public CompleteQuestionnaireSync(ICommandInvoker invoker)
        {
            this.invoker = invoker;
        }

       
        public void Execute()
        {
           ChannelFactory<ICompleteQuestionnaireService> ChannelFactory = new ChannelFactory<ICompleteQuestionnaireService>("test");
            ICompleteQuestionnaireService client = ChannelFactory.CreateChannel();
            Guid syncKey = Guid.NewGuid();

            try
            {
                var store = new DocumentStore() { Url = "http://localhost:8080" }.Initialize();
                var session = store.OpenSession();
                var events =
                    session.Query<EventDocument>();
                foreach (var eventItem in events)
                {

               /*     var settings = new JsonSerializerSettings();
                    settings.TypeNameHandling = TypeNameHandling.Objects;*/
                    var message = new EventSyncMessage
                    {
                        SynchronizationKey = syncKey,
                        Command = eventItem.Command
                    };


                    ErrorCodes returnCode = client.Process(message);
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
