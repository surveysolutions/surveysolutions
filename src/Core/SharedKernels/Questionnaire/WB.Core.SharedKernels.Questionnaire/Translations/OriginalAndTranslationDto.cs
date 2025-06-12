using System;

namespace WB.Core.SharedKernels.Questionnaire.Translations;

public class OriginalAndTranslationDto
{
    public virtual TranslationType? Type { get; set; }

    public virtual Guid? QuestionnaireEntityId { get; set; }

    public virtual string? TranslationIndex { get; set; }
        
    public virtual string? OriginalText { get; set; }
    public virtual string? TranslationText { get; set; }
}
