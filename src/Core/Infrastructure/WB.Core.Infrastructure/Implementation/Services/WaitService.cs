using System;
using WB.Core.Infrastructure.Services;

namespace WB.Core.Infrastructure.Implementation.Services
{
    public class WaitService : IWaitService
    {
        public async void WaitForSeconds(int seconds)
        {
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(seconds));
        }

        public void WaitForSecond()
        {
            this.WaitForSeconds(1);
        }
    }
}
