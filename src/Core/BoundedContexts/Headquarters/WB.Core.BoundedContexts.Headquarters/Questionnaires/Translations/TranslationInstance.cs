using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations
{
    public class TranslationInstance : TranslationDto
    {
        public virtual int Id { get; set; }

        public virtual QuestionnaireIdentity QuestionnaireId { get; set; }
    }
}