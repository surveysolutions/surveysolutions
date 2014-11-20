using System;

namespace WB.Core.Infrastructure
{
    public static class WaitUtils
    {
        public static async void WaitForSecond()
        {
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}