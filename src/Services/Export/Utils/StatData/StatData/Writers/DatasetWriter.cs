using System.IO;
using StatData.Core;

//! Implementation of binary writers to statistical packages' formats.
namespace StatData.Writers
{
    public abstract class DatasetWriter : IDatasetWriter
    {
        #region Implementation of IDatasetWriter

        public abstract void WriteToStream(Stream stream, IDatasetMeta meta, string[,] data);

        public void WriteToFile(string filename, IDatasetMeta datasetMeta, string[,] data)
        {
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                WriteToStream(stream, datasetMeta, data);
                stream.Close(); // redundant according to http://stackoverflow.com/questions/1079434/do-we-need-to-close-a-c-sharp-binarywriter-or-binaryreader-in-a-using-block
            }
        }
        
        public void WriteToFile(string filename, string[,] data)
        {
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                WriteToStream(stream, data);
                stream.Close(); // redundant according to http://stackoverflow.com/questions/1079434/do-we-need-to-close-a-c-sharp-binarywriter-or-binaryreader-in-a-using-block
            }
        }

        public void WriteToStream(Stream stream, string[,] data)
        {
            var dq = new StringArrayDataQuery(data);
            var meta = DatasetMeta.FromData(dq);
            WriteToStream(stream, meta, data);
        }

        public abstract void WriteToStream(Stream stream, IDatasetMeta meta, IDataQuery data);

        public void WriteToStream(Stream stream, IDataQuery data)
        {
            var meta = DatasetMeta.FromData(data);
            WriteToStream(stream, meta, data);
        }

        public void WriteToFile(string filename, IDataQuery data)
        {
            var meta = DatasetMeta.FromData(data);
            WriteToFile(filename, meta, data);
        }

        public void WriteToFile(string filename, IDatasetMeta meta, IDataQuery data)
        {
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                WriteToStream(stream, meta, data);
                stream.Close(); // redundant according to http://stackoverflow.com/questions/1079434/do-we-need-to-close-a-c-sharp-binarywriter-or-binaryreader-in-a-using-block
            }
        }

        public event ProgressChangedDelegate OnProgressChanged;

        #endregion

        
        private int? _percent;
        internal void ChangeProgress(double percent)
        {
            if (_percent.HasValue && _percent == (int) percent) return;

            _percent = (int) percent;

            if (OnProgressChanged != null)
                OnProgressChanged(this, (new ProgressChangedArgs {Progress = (int) percent}));
        }
    }
}

/*
    Consider adding the following checks in this library:
 - culture is always set to something (not null);
 - for every variable there is a recommended storage type different from 'unknown';
 
    Process re-organization:
 
 - allow export to tab+x+y+z+... = tab data + various header files;
 
 - export from Designer - there should be sufficient information in 
   the questionnaire to properly construct the headers and dump with 
   empty data;
 
 */
