using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Designer.Services
{
    public class QuestionnaireDowngradeService
    {
        public virtual void Downgrade(QuestionnaireDocument document, Version targetVersion)
        {
            if (targetVersion < new Version(12, 0, 0))
            {
                document.Children.ForEach(
                    c => c.TreeToEnumerable(q => q.Children).OfType<AbstractQuestion>().ForEach(
                        v =>
                        {
                            v.ValidationMessage = v.ValidationConditions.FirstOrDefault()?.Message;
                        }));
            }
        }
    }
}