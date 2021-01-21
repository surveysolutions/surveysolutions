namespace StatData.Core
{
    /// <summary>
    /// Represents essence of a dataset variable
    /// </summary>
    public interface IDatasetVariable
    {
        /// <summary>
        /// Name the variable (read-only)
        /// </summary>
        string VarName { get; }

        /// <summary>
        /// Label of the variable (optional).
        /// </summary>
        string VarLabel { get; set; }

        /// <summary>
        /// Recommended storage type (optional).
        /// 
        /// Defaults to 'unknown'.
        /// </summary>
        VariableStorage Storage { get; }

        /// <summary>
        /// For numeric variables, desirable number of decimal digits. Default is 8.
        /// </summary>
        int? FormatDecimals { get; set; }
    }
}
