using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace SynchronizationMessages.CompleteQuestionnaire
{
    [JsonNewSerializerContractBehavior]
    [ServiceContract]
    public interface IGetAggragateRootList
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IGetAggragateRootList/Process", ReplyAction = "http://tempuri.org/IGetAggragateRootList/ProcessResponse")]
        ListOfAggregateRootsForImportMessage Process();
    }
}
