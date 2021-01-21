using System;
using System.Collections.Generic;
using System.IO;
using StatData.Converters;
using StatData.Core;

//! This functionality is not standardized yet
namespace StatData.Readers
{
    /// <summary>
    /// Reader of tab-delimited text
    /// </summary>
    public class TabReader:IDatasetReader
    {
        const char Separator = '\t';

        /// <summary>
        /// Reads variable names from a tab-delimited file.
        /// </summary>
        /// <param name="filename">File on disk (must exist).</param>
        /// <returns>List of variable names as strings.</returns>
        public static List<string> GetVarNames(string filename)
        {
            var header = ReadVarNamesStr(filename);
            return header == null
                       ? new List<string>()
                       : new List<string>(header.Split(Separator));
        }

        /// <summary>
        /// Reads variable names from a tab-delimited file.
        /// </summary>
        /// <param name="filename">File on disk (must exist).</param>
        /// <returns>Meta information recovered from the file.</returns>
        public static IDatasetMeta GetMeta(string filename)
        {
            return DatasetMeta.FromVarlist(GetVarNames(filename));
        }
       
        /// <summary>
        /// Returns the list of variables contained in the file
        /// </summary>
        /// <param name="filename">Source file (must exist)</param>
        /// <returns>Space or tab-delimited list of variables. Parse for both delimiters when processing.</returns>
        public string GetVarNamesStr(string filename)
        {
            return ReadVarNamesStr(filename);
        } 
        
        internal static string ReadVarNamesStr(string filename)
        {
            using (var tr = new StreamReader(filename))
            {
                var result = tr.ReadLine();
                tr.Close();
                return result;
            }
        }

        /// <summary>
        /// Reads string matrix from tab-delimited file.
        /// 
        /// First line is interpreted as variable names and ignored.
        /// Each line of file must have same number of delimiters as the header.
        /// File must exist and be non-empty.
        /// </summary>
        /// <param name="filename">File on disk (must exist)</param>
        /// <returns>Matrix of strings</returns>
        public string[,] GetData(string filename)
        {
            var dimensions = GetDimensions(filename);

            // Read data
            using (var tr = new StreamReader(filename))
            {
                var data = ReserveMatrix(dimensions);
                tr.BaseStream.Seek(0, SeekOrigin.Begin);
                tr.ReadLine(); //trash header

                for (var r = 0; r < dimensions.ObsCount; r++)
                {
                    var dataLine = tr.ReadLine().Split(Separator);
                    for (var c = 0; c < dimensions.VarCount; c++)
                        data[r, c] = dataLine[c];
                }
                tr.Close();

                return data;
            }
        }

        internal static string[,] ReserveMatrix(DataDimensions dimensions)
        {
            return new string[dimensions.ObsCount, dimensions.VarCount];
        }

        internal static DataDimensions GetDimensions(string filename)
        {
            var tr = new StreamReader(filename);
            var header = tr.ReadLine();
            if (String.IsNullOrWhiteSpace(header)) return null; // nothing to convert --> exit

            var colCount = header.Split(Separator).Length;

            // Count number of data rows
            var rowCount = 0;
            while (!tr.EndOfStream)
            {
                tr.ReadLine();
                rowCount++;
            }
            tr.Close();

            var result = new DataDimensions { ObsCount = rowCount, VarCount = colCount };
            return result;
        }


    }
}
