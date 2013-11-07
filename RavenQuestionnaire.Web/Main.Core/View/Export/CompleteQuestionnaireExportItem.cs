using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;

namespace Main.Core.View.Export
{
    public class CompleteQuestionnaireExportItem
    {
        public CompleteQuestionnaireExportItem(ICompleteGroup document, IEnumerable<Guid> headerKey, Guid? parent)
        {
            this.PublicKey = document.Propagated == Propagate.None ? document.PublicKey : document.PropagationPublicKey.Value;
            this.Parent = parent;
            this.Values = new ValueCollection();

            foreach (Guid key in headerKey)
            {
                var question = document.FirstOrDefault<AbstractQuestion>(c => c.PublicKey == key);
               // this.Values.Add(key, question);
            }
        }

        public ValueCollection Values { get; set; }

        public Guid PublicKey { get; private set; }
        public Guid? Parent { get; private set; }
    }
}