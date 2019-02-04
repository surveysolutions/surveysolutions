using System;
using Main.Core.Entities.Composite;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public interface IQuestionnaireSearchStorage
    {
        void AddOrUpdateEntity(Guid questionnaireId, IComposite composite);
        void Remove(Guid questionnaireId, Guid entityId);
        SearchResult Search(SearchInput input);
        void RemoveAllEntities(Guid questionnaireId);
    }
}