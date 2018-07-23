using Android.App;
using Android.OS;

namespace WB.UI.Shared.Enumerator.Services
{
    public class ServiceBinder<T> : Binder where T: Service
    {
        private readonly T service;

        public ServiceBinder(T service)
        {
            this.service = service;
        }

        public T GetService()
        {
            return service;
        }
    }
}
