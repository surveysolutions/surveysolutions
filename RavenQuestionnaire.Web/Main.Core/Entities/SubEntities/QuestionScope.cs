// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionType.cs" company="">
//   
// </copyright>
// <summary>
//   The question type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities
{
    /// <summary>
    /// The question type.
    /// </summary>
    public enum QuestionScope
    {
        /// <summary>
        /// Interviewer can see and edit only his own questions.
        /// </summary>
        Interviewer = 0, 

        /// <summary>
        /// Headquarter can see Interviewer's questions and edit his own.
        /// </summary>
        Supervisor = 1, 

        /// <summary>
        /// Headquarter can see Supervisor's and Interviewer's questions and edit his own.
        /// </summary>
        Headquarter = 2, 
    }
}