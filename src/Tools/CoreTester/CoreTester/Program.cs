using System;
using System.Data.Common;
using CommandLine;
using CoreTester.Commands;
using CoreTester.Setup;
using Ninject;
using NLog;

namespace CoreTester
{
    class Program
    {
        [Verb("run", HelpText = "Test core.")]
        protected class CoreTestOptions
        {
            [Option('c', "connection", Required = true, HelpText = "Connection string to DB")]
            public string ConnectionString { get; set; }
        }

        [Verb("dump", HelpText = "Dump debug information in folder")]
        protected class DumpDebugInformationOptions
        {
            [Option('c', "connection", Required = true, HelpText = "Connection string to DB")]
            public string ConnectionString { get; set; }
        }

        [Verb("debug", HelpText = "Debug interviews dumped in folder with 'dump -c' or 'run -c' command")]
        protected class CoreDebugOptions
        {
            [Option('f', "folder", Required = true, HelpText = "Folder with questionnaire, assembly and interviews")]
            public string Folder{ get; set; }
        }

        [Verb("orphans", HelpText = "Remove orphan records from export files.")]
        protected class RemoveOrphanInterviewRecordsOptions
        {
            [Option('c', "connection", Required = true, HelpText = "Connection string to DB")]
            public string ConnectionString { get; set; }
        }

        static int Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Application started");

            try
            {
                return Parser.Default
                    .ParseArguments<CoreTestOptions, DumpDebugInformationOptions, CoreDebugOptions, RemoveOrphanInterviewRecordsOptions>(args)
                    .MapResult(
                        (CoreTestOptions o) => RunCoreTestOptions(o),
                        (DumpDebugInformationOptions o) => RunDumpDebugInformationOptions(o),
                        (CoreDebugOptions o) => RunCoreDebugger(o),
                        (RemoveOrphanInterviewRecordsOptions o) => RunOrphanInterviewRecordsRemover(o),
                        errs =>
                        {
                            foreach (var error in errs)
                            {
                                Console.WriteLine(error);
                            }

                            return 1;
                        });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                logger.Error(e);
                return 1;
            }
        }

        private static int RunOrphanInterviewRecordsRemover(RemoveOrphanInterviewRecordsOptions opts)
        {
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"started at {DateTime.Now}");
            DbConnectionStringBuilder db = new DbConnectionStringBuilder {ConnectionString = opts.ConnectionString};
            var serverName = db["Database"].ToString();
            Console.WriteLine(serverName);
            Console.WriteLine();

            IKernel container = NinjectConfig.CreateKernel(opts.ConnectionString.Trim('"'));

            RemoveOrphanInterviewRecords remover = container.Get<RemoveOrphanInterviewRecords>();

            var runResult = remover.Run(serverName);

            Console.WriteLine();
            Console.WriteLine("Press Any key");
            Console.ReadLine();

            return runResult;
        }

        private static int RunCoreDebugger(CoreDebugOptions opts)
        {
            IKernel container = NinjectConfig.CreateKernelForDebug();

            CoreDebugger dumper = container.Get<CoreDebugger>();

            var runResult = dumper.Run(opts.Folder);

            Console.WriteLine();
            Console.WriteLine("Press Any key");
            Console.ReadLine();

            return runResult;
        }

        private static int RunDumpDebugInformationOptions(DumpDebugInformationOptions opts)
        {
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"started at {DateTime.Now}");
            DbConnectionStringBuilder db = new DbConnectionStringBuilder {ConnectionString = opts.ConnectionString};
            var serverName = db["Database"].ToString();
            Console.WriteLine(serverName);
            Console.WriteLine();

            IKernel container = NinjectConfig.CreateKernel(opts.ConnectionString.Trim('"'));

            DebugInformationDumper dumper = container.Get<DebugInformationDumper>();

            return dumper.Run(serverName);
        }

        private static int RunCoreTestOptions(CoreTestOptions opts)
        {
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"started at {DateTime.Now}");
            DbConnectionStringBuilder db = new DbConnectionStringBuilder {ConnectionString = opts.ConnectionString};
            var serverName = db["User Id"].ToString();
            Console.WriteLine(serverName);
            Console.WriteLine();

            IKernel container = NinjectConfig.CreateKernel(opts.ConnectionString.Trim('"'));

            CoreTestRunner coreTestRunner = container.Get<CoreTestRunner>();

            DebugInformationDumper dumper = container.Get<DebugInformationDumper>();

            var result = coreTestRunner.Run(serverName);
            dumper.Run(serverName);

            return result;
        }
    }
}
