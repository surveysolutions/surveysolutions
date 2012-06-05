using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace SynchronizationMessages.Discover
{
    [ServiceContract]
    public interface ISpotSync
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ISpotSync/Process", ReplyAction = "http://tempuri.org/ISpotSync/ProcessResponse")]
        string Process();
    }
}
