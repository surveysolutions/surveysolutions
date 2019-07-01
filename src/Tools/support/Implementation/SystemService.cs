using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using support.Services;
using System.ServiceProcess;

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

        private bool ServiceExists(string ServiceName)
        {
            return ServiceController.GetServices().Any(serviceController => serviceController.ServiceName.Equals(ServiceName));
        }

        public async Task<bool> IsWindowsServiceExist(string serviceName)
        {
            if(!ServiceExists(serviceName))
                return await Task.FromResult(false);

            return await Task.FromResult(true);
        }

        public async Task<bool> IsWindowsServiceRunning(string serviceName)
        {
            ServiceController sc = new ServiceController(serviceName);
            
            if (sc.Status != ServiceControllerStatus.Running)
            {
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }
    }
}
