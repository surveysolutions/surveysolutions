using System;
using System.IO;
using StatData.Core;

namespace StatData.Writers
{
    /// <summary>
    /// Writer into a simple tab-delimited format.
    /// </summary>
    public class TabWriter: DatasetWriter, IDatasetWriter
    {
        #region Overrides of DatasetWriter

        public override void WriteToStream(Stream stream, IDatasetMeta meta, string[,] data)
        {
            var dq = new StringArrayDataQuery(data);
            WriteToStream(stream, meta, dq);
        }

        public override void WriteToStream(Stream stream, IDatasetMeta meta, IDataQuery data)
        {
            using (var bw = new StreamWriter(stream))
            {
                var nCols = meta.Variables.Length;

                // write header
                for (var v = 0; v < nCols; v++)
                {
                    bw.Write(meta.Variables[v].VarName);
                    bw.Write(v < nCols - 1 ? "\t" : Environment.NewLine);
                }

                // write data
                ChangeProgress(0.00);
                for (var i = 0; i < data.GetRowCount(); i++)
                {
                    for (var v = 0; v < nCols; v++)
                    {
                        var value = data.GetValue(i, v);
                        if (value != null) bw.Write(value);
                        bw.Write(v < nCols - 1 ? "\t" : Environment.NewLine);
                    }
                    ChangeProgress(100.00*i/data.GetRowCount());
                }
                ChangeProgress(100.00);
            }
        }

        // draft implementation - not implemented yet
        internal void WriteToStream(Stream stream, IDatasetMeta meta, INamedDataQuery data)
        {
            using (var bw = new StreamWriter(stream))
            {
                var nCols = meta.Variables.Length;
                var positions = new int[nCols];
                var vnames = data.GetVarNames();

                // write header
                for (var v = 0; v < nCols; v++)
                {
                    bw.Write(meta.Variables[v].VarName);
                    bw.Write(v < nCols - 1 ? "\t" : Environment.NewLine);
                    positions[v] = 0;
                    while (vnames[positions[v]] != meta.Variables[v].VarName) 
                        positions[v]++;
                }

                // write data
                ChangeProgress(0.00);
                for (var i = 0; i < data.GetRowCount(); i++)
                {
                    for (var v = 0; v < nCols; v++)
                    {
                        var value = data.GetValue(i, positions[v]);
                        if (value != null) bw.Write(value);
                        bw.Write(v < nCols - 1 ? "\t" : Environment.NewLine);
                    }
                    ChangeProgress(100.00 * i / data.GetRowCount());
                }
                ChangeProgress(100.00);
            }
        }

        #endregion
    }
}
