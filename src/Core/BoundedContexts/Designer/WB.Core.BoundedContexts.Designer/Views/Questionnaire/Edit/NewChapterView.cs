﻿using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewChapterView
    {
        public IQuestionnaireItem Chapter { set; get; }
        public VariableName[] VariableNames { set; get; }
    }
}
