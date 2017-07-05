using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions
{
    public class QuestionnaireCompilationVersionService : IQuestionnaireCompilationVersionService
    {
        private readonly IPlainStorageAccessor<QuestionnaireCompilationVersion> questionnaireCompilationVersionStorage;

        public QuestionnaireCompilationVersionService(IPlainStorageAccessor<QuestionnaireCompilationVersion> questionnaireCompilationVersionStorage)
        {
            this.questionnaireCompilationVersionStorage = questionnaireCompilationVersionStorage;
        }

        public IEnumerable<QuestionnaireCompilationVersion> GetCompilationVersions()
        {
            return this.questionnaireCompilationVersionStorage.Query(_ => _.ToList());
        }

        public void Update(QuestionnaireCompilationVersion version)
        {
            this.questionnaireCompilationVersionStorage.Store(version, version.QuestionnaireId);
        }

        public void Remove(Guid questionnaireId)
        {
            this.questionnaireCompilationVersionStorage.Remove(questionnaireId);
        }

        public void Add(QuestionnaireCompilationVersion version)
        {
            this.questionnaireCompilationVersionStorage.Store(version, version.QuestionnaireId);
        }

        public QuestionnaireCompilationVersion GetById(Guid id)
        {
            return this.questionnaireCompilationVersionStorage.GetById(id);
        }
    }
}
