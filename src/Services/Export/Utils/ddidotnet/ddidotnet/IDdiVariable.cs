namespace ddidotnet
{
    /// <summary>
    ///     Interface for a DDI variable descriptor
    /// </summary>
    public interface IDdiVariable
    {
        /// <summary>
        ///     Adds an individual value label to the variable
        /// </summary>
        /// <param name="value">Numeric value being labelled.</param>
        /// <param name="label">Label for the specified value.</param>
        /// Value labels are kind of dictionaries that may be applied 
        /// to coded variables to make it easier for humans to work 
        /// with the codes.
        /// 
        /// \image html img\ValueLabels.png "Value labels" width=6cm
        /// \image latex img\ValueLabels.png "Value labels" width=6cm
        /// 
        /// \b Example Adding value labels
        /// \snippet Example2.cs Adding value labels
        void AddValueLabel(decimal value, string label);

        /// <summary>
        ///     Assigns variable to a group.
        ///     If group with specified name does not exist it will be created.
        /// </summary>
        /// <param name="groupName">Name of the group to assign the variable to.</param>
        /// \image html img\DdiVarGroups2.png "Variable groups" width=8cm
        /// \image latex img\DdiVarGroups2.png "Variable groups" width=8cm
        /// 
        /// \sa \ref DdiDataFile.AssignVariableToGroup
        void AssignToGroup(string groupName);
    }
}
