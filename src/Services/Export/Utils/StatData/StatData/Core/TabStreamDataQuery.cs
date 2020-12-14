using System;
using System.IO;

namespace StatData.Core
{
    /// <summary>
    /// Class implementing functionality of an access to a 
    /// tab-delimited file or stream without storing the 
    /// whole data source in memory. 
    /// </summary>
    public class TabStreamDataQuery : IDataQuery
    {
        #region Implementation of IDataQuery

        /// <summary>
        /// Request data from a particular cell of the dataset.
        /// </summary>
        /// <param name="row">row (observation number).</param>
        /// <param name="col">column (variable number).</param>
        /// <returns>Content of the dataset in the specified cell in string format.</returns>
        public string GetValue(long row, int col)
        {
            if (col >= _colCount)
            {
                return String.Empty; // ???? this should not be neccessary ????
            }

            AdvanceTo((int)row);

            return _lineContent[col];
        }

        /// <summary>
        /// Returns number of rows (observations) in the dataset.
        /// </summary>
        /// <returns>Number of rows.</returns>
        public int GetRowCount()
        {
            return _rowCount;
        }

        /// <summary>
        /// Returns number of columns (variables) in the dataset.
        /// </summary>
        /// <returns>Number of columns.</returns>
        public int GetColCount()
        {
            return _colCount;
        }
        
        #endregion

        /// <summary>
        /// Names of the variables in the tab file
        /// </summary>
        /// <returns></returns>
        public string[] GetVarNames()
        {
            return _headerVariables;
        }


        private void AdvanceTo(int row)
        {
            if (_state == ReadState.Ready && _line == row) return;

            if (row > _rowCount)
                throw new ArgumentException("Attempt to seek past end of data file.", "row");

            if ((_state == ReadState.Dirty) || (_state == ReadState.Ready && _line > row))
            {
                _state = ReadState.Dirty;

                _reader.BaseStream.Seek(0, SeekOrigin.Begin);
                _reader.ReadLine();
                _line = -1;
            }

            var s = String.Empty;
            while (_line < row)
            {
                s = _reader.ReadLine();
                _line++;
            }
            _lineContent = s.Split(Separator);

            _state = ReadState.Ready;
        }

        private readonly StreamReader _reader;
        private int _colCount = 9999;
        private int _rowCount = 9999;
        private string _header;
        private string[] _headerVariables;

        private int _line = -1;
        private string[] _lineContent;
        private ReadState _state;

        private const char Separator = '\t';

        private string _filename = "";
        public string GetName()
        {
            return _filename;
        }

        /// <summary>
        /// Constructor, creates an instance of TabStreamDataQuery 
        /// connected to the specified file.
        /// </summary>
        /// <param name="reader">Seekable stream (StreamReader) to read data from.</param>
        public TabStreamDataQuery(StreamReader reader)
        {
            if (reader.BaseStream.CanSeek == false)
                throw new Exception("Stream must be seekable.");

            _reader = reader;
            Inspect();
        }
      
        
        /// <summary>
        /// Constructor, creates an instance of TabStreamDataQuery 
        /// connected to the specified file.
        /// </summary>
        /// <param name="filename">File to read data from.</param>
        public TabStreamDataQuery(string filename)
        {
            var reader = new StreamReader(filename);
            
            if (reader.BaseStream.CanSeek == false)
                throw new Exception("Stream must be seekable.");

            _reader = reader;
            _filename = filename;
            Inspect();
        }

        private void Inspect()
        {
            _reader.BaseStream.Seek(0, SeekOrigin.Begin);
            _state = ReadState.Dirty;

            _header = _reader.ReadLine();
            if (String.IsNullOrWhiteSpace(_header))
            {
                _colCount = 0;
                _rowCount = 0;
                return;
            }

            _headerVariables = _header.Split(Separator);
            _colCount = _headerVariables.Length;

            // Count number of data rows
            _rowCount = 0;
            while (!_reader.EndOfStream)
            {
                _reader.ReadLine();
                _rowCount++;
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Releases allocated resources
        /// </summary>
        public void Dispose()
        {
            _reader.Close();
            _reader.Dispose();
        }

        #endregion
    }

    internal enum ReadState {Ready, Dirty}
}
