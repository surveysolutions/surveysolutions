using System;

namespace WB.UI.WebTester.Services
{
    public interface IEvictionObservable
    {
        IDisposable Subscribe(Action<Guid> action);
    }
}