namespace Main.Core.Entities.Extensions
{
    using System.Linq;

    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class CompleteQuestionExtensions
    {
        /// <summary>
        /// The is value question.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsValueQuestion(this ICompleteQuestion question)
        {
            return question.GetType().GetInterfaces().Any(x => x.IsGenericType &&
                                                               x.GetGenericTypeDefinition()
                                                               == typeof(ICompelteValueQuestion<>));
        }
    }
}
