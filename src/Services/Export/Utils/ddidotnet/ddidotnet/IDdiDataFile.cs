namespace ddidotnet
{
    /// <summary>
    ///     Interface for a DDI data file descriptor.
    /// </summary>
    public interface IDdiDataFile
    {
        /// <summary>
        ///     Creates a new variable in the collection of variables associated with the data file
        /// </summary>
        /// <param name="dt">Variable type</param>
        /// <returns>Reference to the created variable</returns>
        DdiVariable AddVariable(DdiDataType dt);

        /// <summary>
        ///     Assigns the variable to a group of variables.
        ///     If group with specified name does not exist it will be created.
        ///     \note Nesstar Publisher v4.0.9 allows variable groups to have duplicate names, whether related to one file or
        ///     different files. It is not clear whether other software may understand such groups. Hence it is not recommended to
        ///     have duplicate group names anywhere in the DDI document.
        ///     \note Nesstar Publisher v4.0.9 allows empty names for variable groups. This is not recommended.
        /// </summary>
        /// <param name="groupName">Name of the group</param>
        /// <param name="variable">Variable</param>
        /// \b Example Assigning variables to groups
        /// \snippet Example2.cs Assigning variables to groups
        void AssignVariableToGroup(string groupName, DdiVariable variable);

        /// <summary>
        ///     Assigns multiple variables to a group.
        ///     If group with specified name does not exist it will be created.
        ///     \note Nesstar Publisher v4.0.9 allows variable groups to have duplicate names, whether related to one file or
        ///     different files. It is not clear whether other software may understand such groups. Hence it is not recommended to
        ///     have duplicate group names anywhere in the DDI document.
        ///     \note Nesstar Publisher v4.0.9 allows empty names for variable groups. This is not recommended.
        /// </summary>
        /// <param name="groupName">Name of the group</param>
        /// <param name="variables">One or more variables</param>
        /// Group can later be expanded with other variables.
        void AssignVariablesToGroup(string groupName, params DdiVariable[] variables);
    }
}
