using System.IO;
using StatData.Core;
using StatData.Writers.Stata14;

namespace StatData.Writers
{
    /// <summary>
    /// Writes dataset in Stata's [version 14.0] native binary format
    /// 
    /// Writer has the following limitations:
    /// max number of observations 2,147,483,647;
    /// max number of variables 32,767; not all Stata flavors can open more than 2,048 variables;
    /// string variables not to exceed 2045 bytes (at least 511 unicode characters);
    /// unicode is supported.
    /// 
    /// Variable name requirements (from Stata 13 manual, section 11.3 "Naming Conventions"):
    /// A name is a sequence of one to 32 letters (A–Z and a–z), digits (0–9), and underscores (_).
    /// The first character of a name must be a letter
    /// Variable names with an underscore are technically allowed in Stata, but are not desirable, and hence not allowed by this writer.
    /// Stata respects case; that is, myvar, Myvar, and MYVAR are three distinct names.
    /// The following names are reserved and hence disallowed for variable names:
    /// _all float _n _skip _b if _N str# byte in _pi strL _coef int _pred using _cons long _rc with double;
    /// where str# is any name of the type str1, str200, str24746894
    /// 
    /// Although Stata 14 relaxed variable names to include unicode characters, Stata 13 restrictions still apply.
    /// 
    /// </summary>
    public class StataWriter : DatasetWriter, IDatasetWriter
    {
        #region Overrides of DatasetWriter

        public override void WriteToStream(Stream stream, IDatasetMeta meta, string[,] data)
        {
            var dq = new StringArrayDataQuery(data);
            WriteToStream(stream, meta, dq);
        }

        public override void WriteToStream(Stream stream, IDatasetMeta meta, IDataQuery data)
        {
            var helper = new Stata14WriterHelper(meta, data);
            SaveToStream(stream, helper);
        }

        #endregion

        private void SaveToStream(Stream fs, IDataAccessor14 data)
        {
            var internalWriter = new Stata14WriterInternal(data);
            internalWriter.Write(fs, ChangeProgress);
        }
    }

}
