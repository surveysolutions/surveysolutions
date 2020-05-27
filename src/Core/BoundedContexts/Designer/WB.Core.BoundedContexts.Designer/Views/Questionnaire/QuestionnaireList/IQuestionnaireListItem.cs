using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    public interface IQuestionnaireListItem
    {
        Guid PublicId { get; set; }

        string Title { get; set; }
    }
}
