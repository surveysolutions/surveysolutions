﻿namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public interface IQuestionnaireInfoViewFactory
    {
        QuestionnaireInfoView Load(string questionnaireId);
    }
}