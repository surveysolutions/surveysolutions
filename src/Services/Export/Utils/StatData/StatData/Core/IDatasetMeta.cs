using System;
using System.Collections.Generic;
using System.Globalization;

namespace StatData.Core
{
    /// <summary>
    /// Dataset common properties
    /// </summary>
    public interface IDatasetMeta
    {
        /// <summary>
        /// All variables of the dataset
        /// </summary>
        IDatasetVariable[] Variables { get; }
        /// <summary>
        /// Optional data file label (string, default: empty)
        /// </summary>
        string DataLabel { get; set; }

        /// <summary>
        /// Optional data file timestamp (DateTime, default: current)
        /// </summary>
        DateTime TimeStamp { get; set; }

        /// <summary>
        /// Optional most appropriate script
        ///
        /// Writers are expected to write unicode text if supported.
        /// If unicode is not supported by the writer, writer will look for this recommendation.
        /// If script is set to 'other' writer is expected to take whatever it's built-in default.
        /// </summary>
        DatasetScript AppropriateScript { get; set; }

        /// <summary>
        /// Associates value labels with an SPSS variable
        /// </summary>
        /// <param name="varName">Name of the SPSS variable</param>
        /// <param name="valueSet">Value set to be associated with this variable</param>
        void AssociateValueSet(string varName, ValueSet valueSet);

        /// <summary>
        /// Returns the list of all labelled variables.
        /// </summary>
        /// <returns>List of variable names (strings) to which value label sets were applied.</returns>
        List<string> GetLabelledVariables();

        /// <summary>
        /// Allows retrieval of a particular valueset associated with a particular variable
        /// </summary>
        /// <param name="varname">variable name</param>
        /// <returns>Associated value set or null if nothing is associated with specified variable or variable does not exist.</returns>
        ValueSet GetValueSet(string varname);

        /// <summary>
        /// Sorts the variables to match the specified order
        /// </summary>
        /// <param name="varlist">Space or tab-delimited variable names</param>
        void Sort(string varlist);

        /// <summary>
        /// Sets a commentary associated with the dataset meta (ASCII only!)
        /// </summary>
        /// <param name="comment">commentary text</param>
        void SetAsciiComment(string comment);

        /// <summary>
        /// Returns the commentary associated with the dataset meta (ASCII only!)
        /// </summary>
        /// <returns></returns>
        string GetAsciiComment();

        /// <summary>
        /// Stores and manipulates extended missing values.
        /// </summary>
        IExtendedMissingValuesFacility ExtendedMissings { get; }

        /// <summary>
        /// Stores and manipulates extended string missing values.
        /// </summary>
        IExtendedMissingValuesFacility ExtendedStrMissings { get; }

        /// <summary>
        /// Stores CultureInfo information used for conversion, e.g. strings to numbers.
        /// Default is InvariantCulture.
        /// </summary>
        CultureInfo Culture { get; set; }
    }

    
}