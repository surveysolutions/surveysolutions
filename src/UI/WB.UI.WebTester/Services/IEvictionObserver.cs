using System;

namespace WB.UI.WebTester.Services
{
    public interface IEvictionObserver
    {
        void Evict(Guid token);
    }
}