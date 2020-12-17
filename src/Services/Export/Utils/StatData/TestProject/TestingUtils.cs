using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject
{
    /// <summary>
    /// Utilities to be used in tests.
    /// Do not place any tests in this class!
    /// </summary>
    public class TestingUtils
    {
        /// <summary>
        /// Starts Stata to validate the data created by the test
        /// </summary>
        /// <param name="dofile">Name of the verification script (do-file)</param>
        internal static void StartStata(string dofile)
        {
            var stataPath = // @"c:\Stata14\StataMP.exe"; // alternate
                @"c:\D\Program Files (x86)\Stata14\StataMP-64.exe"; // devstation

            if (!File.Exists(stataPath))
            {
                Assert.Inconclusive("Stata executable not present to run this test!");
                return;
            }

            // now start Stata and execute the test script
            var stataProcess =
                new Process
                    {
                        StartInfo =
                            {
                                WorkingDirectory = Directory.GetCurrentDirectory(),
                                FileName = stataPath,
                                Arguments = " do " + dofile
                            }
                    };
            stataProcess.Start();

            stataProcess.WaitForExit();
        }


        internal static void RunStataTest(string dofile, string markerfile)
        {
            StartStata(dofile);
            Assert.AreEqual(true, File.Exists(markerfile));
            // and delete the marker file that we no longer need
            File.Delete(markerfile);
        }
    }



}
