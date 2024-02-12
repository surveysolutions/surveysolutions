using System;
using System.Runtime.ExceptionServices;

namespace WB.UI.Designer.Exceptions;

public class ClientException : Exception
{
    public ClientException(ClientErrorModel errorModel) : base(errorModel.Message)
    {
        Source  = errorModel.AdditionalData["component"];
            
        foreach (var key in errorModel.AdditionalData.Keys)
        {
            Data[key] = errorModel.AdditionalData[key];
        }
            
        ExceptionDispatchInfo.SetRemoteStackTrace(this, errorModel.AdditionalData["stack"]);
    }
}
