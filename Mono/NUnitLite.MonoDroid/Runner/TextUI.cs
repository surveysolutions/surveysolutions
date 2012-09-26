// *****************************************************
// Copyright 2007, Charlie Poole
//
// Licensed under the Open Software License version 3.0
// *****************************************************

using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace NUnitLite.Runner
{
    /// <summary>
    /// A version of TextUI that outputs to the console.
    /// If you use it on a device without a console like
    /// PocketPC or SmartPhone you won't see anything!
    /// 
    /// Call it from your Main like this:
    ///   new ConsoleUI().Execute(args);
    /// </summary>
    public class ConsoleUI : TextUI
    {
#if NETCF_1_0
        public ConsoleUI() : base(ConsoleWriter.Out) { }
#else
        public ConsoleUI() : base(Console.Out) { }
#endif
    }

    /// <summary>
    /// A version of TextUI that writes to a file.
    /// 
    /// Call it from your Main like this:
    ///   new FileUI(filePath).Execute(args);
    /// </summary>
    public class FileUI : TextUI
    {
        public FileUI(string path) : base(new StreamWriter(path)) { }
    }

    /// <summary>
    /// A version of TextUI that displays to debug.
    /// 
    /// Call it from your Main like this:
    ///   new DebugUI().Execute(args);
    /// </summary>
    public class DebugUI : TextUI
    {
        public DebugUI() : base(DebugWriter.Out) { }
    }

    public class TcpUI : TextUI
    {
        public TcpUI(string hostName, int port) : base( new TcpWriter(hostName, port) ) { }

        public TcpUI(string hostName) : this(hostName, 9000) { }

        public TcpUI() : this("localhost", 9000) { }
    }

    /// <summary>
    /// TextUI is a general purpose class that runs tests and
    /// outputs to a TextWriter.
    /// 
    /// Call it from your Main like this:
    ///   new TextUI(textWriter).Execute(args);
    /// </summary>
    public class TextUI
    {
        private CommandLineOptions options;
        private int reportCount = 0;

        private ArrayList assemblies = new ArrayList();

        private TextWriter writer;

        private TestRunner runner = new TestRunner();

        #region Constructors
        public TextUI(TextWriter writer)
        {
            this.writer = writer;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Execute a test run based on the aruments passed
        /// from Main.
        /// </summary>
        /// <param name="args">An array of arguments</param>
        public bool Execute(string[] args)
        {
            var success = true;

            // NOTE: This must be directly called from the
            // test assembly in order for the mechanism to work.
            var callingAssembly = Assembly.GetEntryAssembly();

            options = ProcessArguments( args );

            if (!options.Help && !options.Error)
            {
                if (options.Wait && !(this is ConsoleUI))
                    writer.WriteLine("Ignoring /wait option - only valid for Console");

                try
                {
                    foreach (string name in options.Parameters) assemblies.Add(Assembly.Load(name));

                    if (assemblies.Count == 0) assemblies.Add(callingAssembly);

                    foreach (Assembly assembly in assemblies)
                    {
                        var thisRunSuccess = true;

                        if (options.TestCount == 0) thisRunSuccess = Run(assembly);
                        else thisRunSuccess = Run(assembly, options.Tests);

                        if (!thisRunSuccess) success = false;
                    }
                }
                catch (TestRunnerException ex)
                {
                    writer.WriteLine(ex.Message);
                    success = false;
                }
                catch (FileNotFoundException ex)
                {
                    writer.WriteLine(ex.Message);
                    success = false;
                }
                catch (Exception ex)
                {
                    writer.WriteLine(ex.ToString());
                    success = false;
                }
                finally
                {
                    if (options.Wait && this is ConsoleUI)
                    {
                        Console.WriteLine("Press Enter key to continue . . .");
                        Console.ReadLine();
                    }
                }
            }

            return success;
        }

        private bool Run(Assembly assembly)
        {
            return ReportResults( runner.Run(assembly) );
        }

        private bool Run(Assembly assembly, string[] tests)
        {
            return ReportResults( runner.Run(assembly, tests) );
        }

        private bool ReportResults( TestResult result )
        {
            bool success = true;
            var summary = new ResultSummary(result);

            writer.WriteLine("\r\n\r\n{0} Tests : {1} Errors, {2} Failures, {3} Not Run\r\n",
                summary.TestCount, summary.ErrorCount, summary.FailureCount, summary.NotRunCount);

            if (summary.ErrorCount + summary.FailureCount > 0)
            {
                PrintErrorReport(result);
                success = false;
            }

            if (summary.NotRunCount > 0) PrintNotRunReport(result);

            return success;
        }
        #endregion

        #region Helper Methods
        private CommandLineOptions ProcessArguments(string[] args)
        {
            this.options = new CommandLineOptions();
            options.Parse(args);

            if (!options.Nologo)
                WriteCopyright();

            if (options.Help)
                writer.Write(options.HelpText);
            else if (options.Error)
                writer.WriteLine(options.ErrorMessage);

            return options;
        }

        private void WriteCopyright()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            System.Version version = executingAssembly.GetName().Version;

#if NETCF_1_0
            writer.WriteLine("NUnitLite version {0}", version.ToString() );
            writer.WriteLine("Copyright 2007, Charlie Poole");
#else
            object[] objectAttrs = executingAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            AssemblyProductAttribute productAttr = (AssemblyProductAttribute)objectAttrs[0];

            objectAttrs = executingAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            AssemblyCopyrightAttribute copyrightAttr = (AssemblyCopyrightAttribute)objectAttrs[0];

            writer.WriteLine(String.Format("{0} version {1}", productAttr.Product, version.ToString(3)));
            writer.WriteLine(copyrightAttr.Copyright);
#endif
            writer.WriteLine();

            string clrPlatform = Type.GetType("Mono.Runtime", false) == null ? ".NET" : "Mono";
            writer.WriteLine("Runtime Environment -");
            writer.WriteLine("    OS Version: {0}", Environment.OSVersion);
            writer.WriteLine("  {0} Version: {1}", clrPlatform, Environment.Version);
            writer.WriteLine();
        }

        private void PrintErrorReport(TestResult result)
        {
            reportCount = 0;
            writer.WriteLine();
            writer.WriteLine("Errors and Failures:");
            PrintErrorResults(result);
        }

        private void PrintErrorResults(TestResult result)
        {
            if (result.Results.Count > 0)
                foreach (TestResult r in result.Results)
                    PrintErrorResults(r);
            else if (result.IsError || result.IsFailure)
            {
                writer.WriteLine();
                writer.WriteLine("{0}) {1} ({2})", ++reportCount, result.Test.Name, result.Test.FullName);
                if (options.ListProperties)
                    PrintTestProperties(result.Test);
                writer.WriteLine(result.Message);
#if !NETCF_1_0
                writer.WriteLine(result.StackTrace);
#endif
            }
        }

        private void PrintNotRunReport(TestResult result)
        {
            reportCount = 0;
            writer.WriteLine();
            writer.WriteLine("Errors and Failures:");
            PrintNotRunResults(result);
        }

        private void PrintNotRunResults(TestResult result)
        {
            if (result.Results != null)
                foreach (TestResult r in result.Results)
                    PrintNotRunResults(r);
            else if (result.ResultState == ResultState.NotRun)
            {
                writer.WriteLine();
                writer.WriteLine("{0}) {1} ({2}) : {3}", ++reportCount, result.Test.Name, result.Test.FullName, result.Message);
                if (options.ListProperties)
                    PrintTestProperties(result.Test);
            }
        }

        private void PrintTestProperties(ITest test)
        {
            foreach (DictionaryEntry entry in test.Properties)
                writer.WriteLine("  {0}: {1}", entry.Key, entry.Value);            
        }
        #endregion
    }
}
