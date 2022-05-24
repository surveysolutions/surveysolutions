using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions
{
    public interface IQuestionnaireCompilationVersionService
    {
        IEnumerable<QuestionnaireCompilationVersion> GetCompilationVersions();
        void Update(QuestionnaireCompilationVersion version);
        void Remove(Guid questionnaireId);
        void Add(QuestionnaireCompilationVersion version);
        QuestionnaireCompilationVersion? GetById(Guid questionnaireId);
    }
}
