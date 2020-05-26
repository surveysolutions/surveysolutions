﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace Main.Core.Entities.Composite
{
    public interface IComposite : IQuestionnaireEntity
    {
        ReadOnlyCollection<IComposite> Children { get; set; }

        void SetParent(IComposite? parent);

        T? Find<T>(Guid publicKey) where T : class, IComposite;

        IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class;

        T? FirstOrDefault<T>(Func<T, bool> condition) where T : class;
        
        void ConnectChildrenWithParent();

        IComposite Clone();

        void Insert(int index, IComposite itemToInsert, Guid? parent);

        void RemoveChild(Guid child);

        string VariableName { get; }
    }
}
