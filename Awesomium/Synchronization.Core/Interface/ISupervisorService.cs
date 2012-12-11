// -----------------------------------------------------------------------
// <copyright file="ISupervisorService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Synchronization.Core.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ServiceModel;

    /*/// <summary>
    /// TODO: Update summary.
    /// </summary>
    [ServiceContract]
    public interface ISupervisorService
    {
        [OperationContract]
        string GetPath();

        [OperationContract]
        string AuthorizeDevice(string data);
    }*/

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName = "ISupervisorService")]
    public interface ISupervisorService
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ISupervisorService/GetPath", ReplyAction = "http://tempuri.org/ISupervisorService/GetPathResponse")]
        string GetPath();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ISupervisorService/AuthorizeDevice", ReplyAction = "http://tempuri.org/ISupervisorService/AuthorizeDeviceResponse")]
        bool AuthorizeDevice(byte[] registerData);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ISupervisorService/GetAuthorizationRequests", ReplyAction = "http://tempuri.org/ISupervisorService/GetAuthorizationRequestsResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object[]))]
        object[] GetAuthorizationRequests();
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ISupervisorServiceChannel : ISupervisorService, System.ServiceModel.IClientChannel
    {
    }
}
