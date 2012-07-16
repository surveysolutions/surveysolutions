using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WinFormsSample.adept_part
{
    class Configuration
    {
        private static string RawProgramDirectory()
        {
            string exename = Application.ExecutablePath;
            return Path.GetDirectoryName(exename);
        }

        public static string ProgramDirectory
        {
            get
            {
                var defaultLocation = RawProgramDirectory();
                var homedir = "D:/"; //temp
                var result = homedir + Path.DirectorySeparatorChar;
                return result;
            }
        }
        public static string ConfigFileName()
        {
            var result = ("config.ini");
            return result;
        }

    }
}
