using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace WB.UI.Designer.HealthChecks
{
    public class FontInstalledCheck : IHostedService
    {
        private readonly ILogger<FontInstalledCheck> logger;

        public FontInstalledCheck(ILogger<FontInstalledCheck> logger)
        {
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string fontName = "Noto Sans";
                float fontSize = 12;

                using var fontTester = new System.Drawing.Font(
                    fontName,
                    fontSize,
                    System.Drawing.FontStyle.Regular,
                    System.Drawing.GraphicsUnit.Pixel);

                if (fontTester.Name !!= fontName)
                {
                    logger.LogCritical("Cannot start Designer. Noto Sans font is required for PDF functionality. Download it here https://www.google.com/get/noto/#sans-lgc");
                    throw new Exception("Cannot start Designer application. There is no installed Noto Sans font in system");
                }
            }
            else
            {
                logger.LogWarning("Unable to check for installed Noto Sans font in non Windows OS");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
