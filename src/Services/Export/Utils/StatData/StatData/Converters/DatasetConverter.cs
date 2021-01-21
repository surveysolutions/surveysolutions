using System.IO;
using System.Runtime.CompilerServices;
using StatData.Core;
using StatData.Readers;
using StatData.Writers;

[assembly:InternalsVisibleTo("StatDataTest")]

namespace StatData.Converters
{
    public class DatasetConverter:IDatasetConverter
    {
        public DatasetConverter(IDatasetReader reader, IDatasetWriter writer)
        {
            Reader = reader;
            Writer = writer;
            Writer.OnProgressChanged += Writer_OnProgressChanged;
        }

        void Writer_OnProgressChanged(object sender, ProgressChangedArgs e)
        {
            if (OnProgressChanged != null) 
                OnProgressChanged(this, e);
        }

        public event ProgressChangedDelegate OnProgressChanged;


        /// <summary>
        /// Converts tab-delimited data file to an SPSS system file.
        /// </summary>
        /// <param name="srcFile">Existing tab-delimited file.</param>
        /// <param name="dstFile">SPSS system file (to be created).</param>
        public void Convert(string srcFile, string dstFile)
        {
            // Read Tab-Delimited data file
            var datasetMeta = TabReader.GetMeta(srcFile);
            Convert(srcFile, dstFile, datasetMeta);
        }

        public void Convert(string srcFile, string dstFile, IDatasetMeta meta)
        {
            // Read input data file
            var h = Reader.GetVarNamesStr(srcFile);
            meta.Sort(h);

            using (var sr = new StreamReader(srcFile))
            {
                var dq = new TabStreamDataQuery(sr);

                // Write intended file
                Writer.WriteToFile(dstFile, meta, dq);
            }
        }

        protected IDatasetWriter Writer;
        protected IDatasetReader Reader;
    }
}
