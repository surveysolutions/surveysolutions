using RavenQuestionnaire.Core.Entities.Iterators;

namespace RavenQuestionnaire.Core
{
    public interface IIteratorContainer
    {
        Iterator<TOutput> Create<TDocument, TOutput>(TDocument input);
    }
}