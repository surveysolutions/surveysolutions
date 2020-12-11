namespace StatData.Core
{
    /// <summary>
    /// Describes the storage type of each individual variable
    /// </summary>
    public enum VariableStorage
    {
        UnknownStorage=0, // nothing is known about the values of this variable, the writer should inspect the values to determine the best output format.
        NumericStorage=1, // all values of this variable are known to be numeric and should be written as such if the output format permits.
        StringStorage=2, // at least some of the values of this variable are known to be strings (not numeric) and the variable should be written to output as a string variable.

        
        // forthcoming :
        NumericIntegerStorage=3, // all values of this variable are numeric and integer
        DateStorage=4, // all values of this variable are representing dates in the format YYYY-MM-DD
        DateTimeStorage=5 // all values of this variable are representing timestamps in the format: YYYY-MM-DDTHH:MM:SS (where T is always a letter 'T')

        // ! SPSS writer doesn't know how to take an advantage of the NumericIntegerStorage!
         
    }
}
