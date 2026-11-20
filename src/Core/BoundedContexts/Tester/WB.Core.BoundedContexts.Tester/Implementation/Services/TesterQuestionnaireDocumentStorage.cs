#nullable enable
using System.Collections.Concurrent;
using Main.Core.Documents;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class TesterQuestionnaireDocumentStorage : IPlainKeyValueStorage<QuestionnaireDocument>
    {
        private readonly ConcurrentDictionary<string, QuestionnaireDocument> documents = new();

        public QuestionnaireDocument? GetById(string id)
            => documents.TryGetValue(id, out var document) ? document : null;

        public bool HasNotEmptyValue(string id) => documents.ContainsKey(id);

        public void Remove(string id) => documents.TryRemove(id, out _);

        public void Store(QuestionnaireDocument entity, string id) => documents[id] = entity;
    }
}
