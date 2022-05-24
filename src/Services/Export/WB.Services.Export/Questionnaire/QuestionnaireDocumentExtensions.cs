using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Services.Export.Questionnaire
{
    public static class QuestionnaireDocumentExtensions
    {
        public static IEnumerable<T> Find<T>(this QuestionnaireDocument questionnaire, Func<T, bool> condition) where T : class
            => questionnaire
                .Find<T>()
                .Where(condition.Invoke);

        public static IEnumerable<T> Find<T>(this QuestionnaireDocument questionnaire) where T : class
            => questionnaire
                .Children
                .TreeToEnumerable(composite => composite.Children)
                .Where(child => child is T)
                .Cast<T>();

        public static T? FirstOrDefault<T>(this QuestionnaireDocument questionnaire, Func<T, bool> condition) where T : class
            => questionnaire.Find(condition).FirstOrDefault();
    }
}
