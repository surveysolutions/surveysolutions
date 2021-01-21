using System;

namespace StatData.Core
{
    /// <summary>
    /// Represents an essence of a dataset variable.
    /// </summary>
    public class DatasetVariable:IDatasetVariable
    {
        internal const string TemplVar = "V{0}";
        internal const string TemplSubVar = TemplVar + "_{1}";
        internal const int DecimalDigitsDefault = 8;
        internal const int DecimalDigitsMax = 15;

        #region Implementation of IDatasetVariable

        /// <summary>
        /// Instantiates a dataset variable with a given name (required).
        /// </summary>
        /// <param name="varname">Variable name</param>
        public DatasetVariable(string varname)
        {
            if (String.IsNullOrWhiteSpace(varname)) 
                throw new ArgumentException("Variable name can't be empty");

            VarName = varname;
            VarLabel = String.Empty;
            Storage = VariableStorage.UnknownStorage;
        }

        /// <summary>
        /// Variable name.
        /// 
        /// Variable names in this class are neutral to the file format and will additionally be validated by the writer.
        /// </summary>
        public string VarName { get; private set; }


        /// <summary>
        /// Variable label.
        /// 
        /// Defaults to an empty label.
        /// </summary>
        public string VarLabel { get; set; }


        /// <summary>
        /// Recommended storage type.
        /// 
        /// Defaults to 'unknown'.
        /// </summary>
        public VariableStorage Storage
        {
            get { return _storage; }
            set
            {
                // for compatibility forcefully convert some of the new
                // storage types to the old storage types
                
                VariableStorage t;                
                switch (value)
                {
                        case VariableStorage.NumericIntegerStorage:
                        t = VariableStorage.NumericIntegerStorage;    // proper version 
                        //t = VariableStorage.NumericStorage;         // released version
                        break;

                        case VariableStorage.DateTimeStorage:
                        t = VariableStorage.StringStorage;
                        break;

                        case VariableStorage.DateStorage:
                        t = VariableStorage.StringStorage;
                        break;

                        default:
                        t = value;
                        break;
                }
                _storage = t;
                //_storage = value;
            }
        }

        private VariableStorage _storage;

        //todo: add tests for this
        public int? FormatDecimals
        {
            get { return _formatDecimals; }
            set
            {
                if (value >= 0 && value <= DecimalDigitsMax)
                {
                    _formatDecimals = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(
                        "FormatDecimals", value, "Value is out of range");
                }
            }
        }

        private int? _formatDecimals = null;

        #endregion
    }
}
