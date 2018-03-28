using System;
using System.Reactive.Subjects;

namespace WB.UI.WebTester.Services.Implementation
{
    public class TokenEviction : IEvictionNotifier, IEvictionObservable
    {
        private readonly Subject<Guid> subject;

        public TokenEviction()
        {
            this.subject = new Subject<Guid>();
        }

        public void Evict(Guid token)
        {
            subject.OnNext(token);
        }

        public IDisposable Subscribe(Action<Guid> action)
        {
            return subject.Subscribe(action);
        }
    }
}
