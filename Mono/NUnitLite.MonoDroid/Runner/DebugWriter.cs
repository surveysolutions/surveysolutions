using System;
using System.Diagnostics;
using System.IO;

namespace NUnitLite.Runner
{
    /// <summary>
    /// DebugWriter is a TextWriter that sends it's 
    /// output to Debug. We don't use Trace because
    /// writing to it is not supported in CF.
    /// </summary>
    public class DebugWriter : TextWriter
    {
        private static TextWriter writer;

        public static TextWriter Out
        {
            get
            {
                if (writer == null)  writer = new DebugWriter();
                return writer;
            }
        }

        // JP - Not sure why Debug.Write couldn't be resolved, but it doesn't look like it's called anyway.
        //public override void Write(char value)
        //{
        //    Debug.Write(value);
        //}

        //public override void Write(string value)
        //{
        //    Debug.Write(value);
        //}

        public override void WriteLine(string value)
        {
            Debug.WriteLine(value);
        }

        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.Default; }
        }
    }
}
