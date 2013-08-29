namespace Main.Core.Documents
{
    /// <summary>
    /// The synchronization statistic type.
    /// </summary>
    public enum SynchronizationStatisticType
    {
        /// <summary>
        /// The new questionnaire.
        /// </summary>
        NewQuestionnaire = 1,

        /// <summary>
        /// The questionnaire update.
        /// </summary>
        QuestionnaireUpdate = 2,

        /// <summary>
        /// The new survey.
        /// </summary>
        NewSurvey = 3,

        /// <summary>
        /// The new assignment.
        /// </summary>
        NewAssignment = 4,

        /// <summary>
        /// The assignment changed.
        /// </summary>
        AssignmentChanged = 5,

        /// <summary>
        /// The status changed.
        /// </summary>
        StatusChanged = 6,

        /// <summary>
        /// New user was added
        /// </summary>
        NewUser = 7
    }
}