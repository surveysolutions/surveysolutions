using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace SynchronizationMessages.CompleteQuestionnaire
{
   
    /// <summary>
    /// This is an example of using a service contract interface
    /// instead of a service reference. Make sure to include the
    /// Action / ReplyAction values that correspond to your
    /// service inputs and outputs. A simple way to find out these
    /// values is to host the service and inspect the auto-generated
    /// WSDL by appending ?wsdl to the URL of the service.
    /// </summary>
    [JsonNewSerializerContractBehavior]
    [ServiceContract]
    public interface ICompleteQuestionnaireSync
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICompleteQuestionnaireService/Process", ReplyAction = "http://tempuri.org/ICompleteQuestionnaireService/ProcessResponse")]
        ErrorCodes Process(EventSyncMessage request);
    }
}
