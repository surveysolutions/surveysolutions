using System;
using System.Runtime.ExceptionServices;

namespace WB.UI.Designer.Exceptions;

public class ClientException : Exception
{
    public ClientException(ClientErrorModel errorModel) : base(errorModel.Message)
    {
        if (errorModel.AdditionalData.TryGetValue("component", out var component))
            Source = component;
            
        foreach (var key in errorModel.AdditionalData.Keys)
        {
            Data[key] = errorModel.AdditionalData[key];
        }
            
        if (errorModel.AdditionalData.TryGetValue("stack", out var stack))
            ExceptionDispatchInfo.SetRemoteStackTrace(this, stack);
    }
}
