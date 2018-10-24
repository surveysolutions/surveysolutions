using System;
using Microsoft.AspNet.SignalR.Hubs;

namespace WB.UI.Headquarters.Code
{
    public interface ILifetimeHub : IHub, IDisposable
    {
        event EventHandler OnDisposing;
    }
}
