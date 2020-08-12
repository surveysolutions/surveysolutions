using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Headquarters.SupportTool
{
    public class VersionCommand : Command
    {
        private IHost host;

        public VersionCommand(IHost host) : base("version", "Print application version information")
        {
            this.host = host;

            this.Handler = CommandHandler.Create(Print);
        }

        private void Print()
        {
            var productVersion = host.Services.GetService<IProductVersion>();
            Console.WriteLine("Survey Solutions Headquarters");
            Console.WriteLine(productVersion.ToString());
        }
    }
}
