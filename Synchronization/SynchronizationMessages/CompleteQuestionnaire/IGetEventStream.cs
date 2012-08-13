using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace SynchronizationMessages.CompleteQuestionnaire
{
    [JsonNewSerializerContractBehavior]
    [ServiceContract]
    public interface IGetEventStream
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IGetEventStream/Process", ReplyAction = "http://tempuri.org/IGetEventStream/ProcessResponse")]
        ImportSynchronizationMessage Process(Guid firstEventPulicKey,int length);
    }
}
