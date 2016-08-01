﻿using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.Questionnaire.Documents
{
    public static class QuestionnaireEntityExtensions
    {
        public static string GetTitle(this IQuestionnaireEntity entity)
            => (entity as IQuestion)?.QuestionText
            ?? (entity as IStaticText)?.Text
            ?? (entity as IGroup)?.Title;

        public static void SetTitle(this IQuestionnaireEntity entity, string title)
        {
            var question = entity as IQuestion;
            var staticText = entity as IStaticText;
            var group = entity as IGroup;

            if (question != null) question.QuestionText = title;
            if (staticText != null) staticText.Text = title;
            if (group != null) group.Title = title;
        }
    }
}