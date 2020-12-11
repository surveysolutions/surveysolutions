using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("TestProject")]
//! Facilitation of meta-information
namespace StatData.Core
{
    /// <summary>
    ///  Dataset common properties
    /// </summary>
    public class DatasetMeta : IDatasetMeta
    {
        /// <summary>
        /// Array of all variables that are contained in the meta.
        /// </summary>
        public IDatasetVariable[] Variables
        {
            get { return _variables; }
        }

        private IDatasetVariable[] _variables;
        
        private DatasetScript _script = DatasetScript.Other;

        #region Implementation of IMeta

        private string _dataLabel = "";
        private string _dataComment = "";

        private IExtendedMissingValuesFacility _extendedMissings = new ExtendedMissingValuesFacility();
        /// <summary>
        /// Facility managing extended missing values.
        /// </summary>
        public IExtendedMissingValuesFacility ExtendedMissings
        {
            get { return _extendedMissings; }
        }

        private IExtendedMissingValuesFacility _extendedStrMissings = new ExtendedMissingValuesFacility();
        /// <summary>
        /// Facility managing extended string missing values.
        /// </summary>
        public IExtendedMissingValuesFacility ExtendedStrMissings
        {
            get { return _extendedStrMissings; }
        }

        public void SetAsciiComment(string comment)
        {
            var AsciiComment = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(comment));

            const int maxCommentLength = 32000; // 400 lines of 80 chars in each

            if (AsciiComment.Length > maxCommentLength)
            {
                // trim the comment to be no longer than the limit.
                AsciiComment = AsciiComment.Substring(0, maxCommentLength);   
            }
            
            _dataComment = AsciiComment;
        }

        public string GetAsciiComment()
        {
            return _dataComment;
        }

        /// <summary>
        /// String label of the whole dataset. Optional.
        /// </summary>
        public string DataLabel
        {
            get { return _dataLabel; }
            set { _dataLabel = value; }
        }

        private DateTime _timeStamp = DateTime.Now;

        /// <summary>
        /// Date and time to be stored in the dataset, usually the current date and time. Optional.
        /// </summary>
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        /// <summary>
        /// Most appropriate script for the characters in the dataset. Optional.
        /// 
        /// Writers are expected to write unicode text if supported.
        /// If unicode is not supported by the writer, writer will look for this recommendation.
        /// If script is set to 'other' writer is expected to take whatever it's built-in default.
        /// </summary>
        public DatasetScript AppropriateScript
        {
            get { return _script; }
            set { _script = value; }
        }

        private readonly Dictionary<string, ValueSet> _valueSets = new Dictionary<string, ValueSet>();

        /// <summary>
        /// Associates value labels with a dataset variable. Value sets are optional.
        /// </summary>
        /// <param name="varName">Name of the variable (must be already declared).</param>
        /// <param name="valueSet">Value set to be associated with this variable</param>
        public void AssociateValueSet(string varName, ValueSet valueSet)
        {
            if (_variables.Any(v => v.VarName == varName))
            {
                if (valueSet == null)
                {
                    if (_valueSets.ContainsKey(varName)) _valueSets.Remove(varName);
                }
                else
                {
                    _valueSets[varName] = valueSet;
                }
            }
            else
                throw new ArgumentException(
                    "Can't associate a value set, variable "
                    + varName + " is undefined.",
                    "varName");
        }

        /// <summary>
        /// Returns the list of all labelled variables.
        /// </summary>
        /// <returns>List of variable names (strings) to which value label sets were applied.</returns>
        public List<string> GetLabelledVariables()
        {
            var result = new List<string>(_valueSets.Keys);
            return result;
        }

        /// <summary>
        /// Returns the valueset associated with a particular variable.
        /// </summary>
        /// <param name="varname">variable name</param>
        /// <returns>Requested valueset</returns>
        public ValueSet GetValueSet(string varname)
        {
            return _valueSets.ContainsKey(varname) ? _valueSets[varname] : null;
        }


        /// <summary>
        /// Sorts the variables in meta to match specified order
        /// </summary>
        /// <param name="varlist">Desired order of variables (separate variable names with space or tab)</param>
        public void Sort(string varlist)
        {
            if (string.IsNullOrEmpty(varlist)) 
                throw new ArgumentNullException("Empty list not allowed");

            var l = varlist.Split(new[] {' ', '\t'});
            if (_variables.Length != l.Length) 
                throw new ArgumentException("Variables list must include all the same variables.");

            var holder = new Dictionary<string, IDatasetVariable>();
            foreach (var variable in _variables)
                holder.Add(variable.VarName, variable);

            _variables = new IDatasetVariable[l.Length];
            for (var i = 0; i < l.Length; i++)
            {
                var varname = l[i];
                _variables[i] = holder[varname];
            }
        }

        #endregion
        
        internal DatasetMeta()
        {
            // set default culture;
            Culture = Util.DefaultCulture;
        }

        /// <summary>
        /// Creates minimal meta data structure from data query object
        /// </summary>
        /// <param name="data">Data that will be written to the binary file</param>
        /// <returns>Minimal meta data structure for output</returns>
        public static IDatasetMeta FromData(IDataQuery data)
        {
            if (data == null) throw new ArgumentException("Data required");

            var nObs = data.GetRowCount();
            if (nObs == 0) throw new ArgumentException("Observations required");

            var nVar = data.GetColCount();
            if (nVar == 0) throw new ArgumentException("Variables required");

            var vars = new IDatasetVariable[nVar];
            for (var i = 0; i < nVar; i++)
            {
                var newname = String.Format(DatasetVariable.TemplVar, i);
                vars[i] = new DatasetVariable(newname);
            }

            var meta = new DatasetMeta(vars);
            return meta;
        }

        /// <summary>
        /// Creates minimal meta data structure from data
        /// </summary>
        /// <param name="data">Data to be written to the binary file</param>
        /// <returns>Minimal meta data structure for output</returns>
        public static IDatasetMeta FromData(string[,] data)
        {
            var dq = new StringArrayDataQuery(data);
            return FromData(dq);
        }

        /// <summary>
        /// Constructor, instantiates a meta, based on an array of variables' names
        /// </summary>
        /// <param name="variables">Array of variables that are present in the dataset.</param>
        public DatasetMeta(IDatasetVariable[] variables)
        {
            // set default culture;
            Culture = Util.DefaultCulture;

            if (variables == null) 
                throw new ArgumentNullException("variables can't be null");

            var l = new List<string>();
            foreach (var v in variables)
                if (l.Contains(v.VarName))
                    throw new ArgumentException("Variable names must be unique, encountered repetitive: " + v.VarName);
                else
                    l.Add(v.VarName);

            _variables = variables;

            SetAsciiComment(StatData.Core.Info.Name);
        }

        /// <summary>
        /// Creates minimal metadata structure from variables list string.
        /// </summary>
        /// <param name="varlist">Space-delimited list of variables.</param>
        /// <returns>Meta data structure.</returns>
        public static DatasetMeta FromVarlist(string varlist)
        {
            var variables = varlist.Split(new[] {' ', '\t'});
            return FromVarlist(variables);
        }
         
        /// <summary>
        /// Creates minimal metadata structure from variables list.
        /// </summary>
        /// <param name="variables">List of variables' names (strings).</param>
        /// <returns>Meta data structure.</returns>
        public static DatasetMeta FromVarlist(IList<string> variables)
        {
            if (variables == null) 
                throw new ArgumentNullException("variables can't be null");

            if (variables.Any(String.IsNullOrWhiteSpace))
                throw new ArgumentException("variables can't be empty");

            var n = variables.Count;
            var vars = new IDatasetVariable[n];

            for (var v = 0; v < n; v++)
                vars[v] = new DatasetVariable(variables[v]);

            return new DatasetMeta(vars);
        }


        public CultureInfo Culture { get; set; }


        internal Dictionary<String, List<String>> GetValueSetsGroups()
        {
            // Q1: Q1,Q2,Q3
            // Q5: Q5,Q10
            // ...............

            var result = new Dictionary<String, List<String>>();

            foreach (var pair in _valueSets)
            {
                var varname = pair.Key;
                var set = pair.Value;

                var flag = false;
                foreach (var s in result)
                {
                    if (Util.ValueSetsAreIdentical(set, _valueSets[s.Key]))
                    {
                        // add to the list for the identified key
                        result[s.Key].Add(varname);
                        flag = true;
                        break;
                    }

                }
                if (!flag)
                {
                    // add to the result as the new value set.
                    result.Add(varname, new List<string>());
                    result[varname].Add(varname);
                }
            }

            return result;
        }


        internal Dictionary<String, String> GetValueSetsMapping()
        {
            //  Q1: Q1
            //  Q2: Q1
            //  Q3: Q1
            //  Q5: Q5
            // Q10: Q5
            // ...............

            var result = new Dictionary<String, String>();

            foreach (var pair in _valueSets)
            {
                var varname = pair.Key;
                var set = pair.Value;

                var flag = false;
                foreach (var s in result)
                {
                    if (Util.ValueSetsAreIdentical(set, _valueSets[s.Key]))
                    {
                        // add to the list for the identified key
                        result.Add(varname,s.Key);
                        flag = true;
                        break;
                    }

                }
                if (!flag)
                {
                    // add to the result as the new value set.
                    result.Add(varname, varname);
                }
            }

            return result;
        }
    }
}
