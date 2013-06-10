using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Main.Core;
using NConfig;
using Ncqrs.Commanding.ServiceModel;
using Ninject;

namespace LoadTestDataGenerator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SetupNConfig();

            var kernel = CompositionRoot.Wire(new MainModule());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(kernel.Get<LoadTestDataGenerator>());

        }

        private static void SetupNConfig()
        {
            NConfigurator.UsingFiles(@"Configuration\LoadTestDataGenerator.config").SetAsSystemDefault();
        }
    }
}
