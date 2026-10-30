using System;

namespace WB.UI.WebTester.Services
{
    public interface IEvictionNotifier
    {
        void Evict(Guid token);
        void Complete(Guid token);
    }
}