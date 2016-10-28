using System;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;

// ReSharper disable once CheckNamespace
namespace Main.Core.Documents
{
    public interface IQuestionnaireDocument : IGroup
    {
        DateTime? CloseDate { get; set; }

        DateTime CreationDate { get; set; }

        DateTime LastEntryDate { get; set; }

        DateTime? OpenDate { get; set; }

        Guid? CreatedBy { get; set; }

        bool IsPublic { get; set; }

        void Add(IComposite c, Guid? parentKey, Guid? parentPropagationKey);

        void Remove(Guid itemKey, Guid? propagationKey, Guid? parentPublicKey, Guid? parentPropagationKey);
    }
}