using System;

namespace WB.UI.WebTester.Services
{
    public interface IEvictionObserver
    {
        void OnNext(Guid token);
    }
}