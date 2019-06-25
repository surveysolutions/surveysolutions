using System;
using System.Diagnostics;
using System.Threading.Tasks;
using support.Services;

namespace support.Implementation
{
    public class SystemService : ISystemService
    {
        public async Task<bool> IsProcessRunning(string processName)
        {
            var runningProcess = Process.GetProcessesByName(processName);
            if (runningProcess.Length < 1)
            {
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }
    }
}
