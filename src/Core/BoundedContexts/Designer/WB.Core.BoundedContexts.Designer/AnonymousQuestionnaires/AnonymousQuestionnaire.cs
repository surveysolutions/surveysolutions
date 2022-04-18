using System;

namespace WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;

public class AnonymousQuestionnaire
{
    public Guid QuestionnaireId { get; set; }
    public Guid AnonymousQuestionnaireId { get; set; }
    public bool IsActive { get; set; }
}