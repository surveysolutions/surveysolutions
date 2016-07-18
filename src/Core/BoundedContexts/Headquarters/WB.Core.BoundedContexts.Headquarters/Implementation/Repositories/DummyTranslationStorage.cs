using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    internal class DummyTranslationStorage : ITranslationStorage
    {
        private class DummyTranslation : ITranslation
        {
            public string GetTitle(Guid entityId) => null;
            public string GetInstruction(Guid questionId) => null;
            public string GetAnswerOption(Guid questionId, string answerOptionValue) => null;
            public string GetValidationMessage(Guid entityId, int validationOneBasedIndex) => null;
            public string GetFixedRosterTitle(Guid rosterId, decimal fixedRosterTitleValue) => null;
            public bool IsEmpty() => true;

        }

        public ITranslation Get(QuestionnaireIdentity questionnaire, string language) => new DummyTranslation();
    }
}