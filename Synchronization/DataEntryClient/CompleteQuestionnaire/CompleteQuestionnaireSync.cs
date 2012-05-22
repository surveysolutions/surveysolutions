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
using RavenQuestionnaire.Core.Views.Event;
using SynchronizationMessages.CompleteQuestionnaire;

namespace DataEntryClient.CompleteQuestionnaire
{
    public class CompleteQuestionnaireSync
    {
        private ICommandInvoker invoker;
        private IViewRepository viewRepository;
        public CompleteQuestionnaireSync(ICommandInvoker invoker, IViewRepository viewRepository)
        {
            this.invoker = invoker;
            this.viewRepository = viewRepository;
        }

       
        public void Execute()
        {
           ChannelFactory<ICompleteQuestionnaireService> ChannelFactory = new ChannelFactory<ICompleteQuestionnaireService>("test");
            ICompleteQuestionnaireService client = ChannelFactory.CreateChannel();
            Guid syncKey = Guid.NewGuid();

            try
            {
                var events = viewRepository.Load<EventBrowseInputModel, EventBrowseView>(new EventBrowseInputModel(null));
                foreach (var eventItem in events.Items)
                {
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
