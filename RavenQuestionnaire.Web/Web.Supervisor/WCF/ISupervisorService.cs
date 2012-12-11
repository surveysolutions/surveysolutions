using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Main.Core.Entities;

namespace Web.Supervisor.WCF
{
    public interface IAuthorizationRequest
    {
        RegisterData Data { get; }
        bool IsAuthorized { get; }
    }

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISupervisorService" in both code and config file together.
    [ServiceContract]
    public interface ISupervisorService
    {
        [OperationContract]
        string GetPath();

        [OperationContract]
        bool AuthorizeDevice(byte[] registerData);

        [OperationContract]
        IList<IAuthorizationRequest> GetAuthorizationRequests();
    }
}
