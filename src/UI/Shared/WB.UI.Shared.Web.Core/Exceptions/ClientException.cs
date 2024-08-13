using System;
using System.Runtime.ExceptionServices;

namespace WB.UI.Shared.Web.Exceptions;

public class ClientException : Exception
{
    public ClientException(ClientErrorModel errorModel) : base(errorModel.Message)
    {
        string fullInfo = string.Empty;

        if (errorModel.AdditionalData.TryGetValue("component", out var component))
            Source = component;
            
        foreach (var key in errorModel.AdditionalData.Keys)
        {
            var value = errorModel.AdditionalData[key];
            Data[key] = value;
            fullInfo += $"{key}: {value}\r\n";
        }
            
        if (!string.IsNullOrWhiteSpace(fullInfo))
            ExceptionDispatchInfo.SetRemoteStackTrace(this, fullInfo);
    }

    public ClientException(string error, string json) : base(error)
    {
        if (json != null)
            ExceptionDispatchInfo.SetRemoteStackTrace(this, json);
    }
}
