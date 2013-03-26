namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.Validation
{
    public interface IQuestionnaireValidationExecutor
    {
        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        bool Execute();

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        bool Execute(QuestionViewModel question);
    }
}