using System;

namespace WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;

public class AnonymousQuestionnaire
{
    public  int Id { get; set; }

    public Guid QuestionnaireId { get; set; }
    public Guid? AnonymousQuestionnaireId { get; set; }
    public bool IsActive { get; set; }
}