using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Designer.Services
{
    public class QuestionnaireDowngradeService
    {
        public virtual void Downgrade(QuestionnaireDocument document, int targetVersion)
        {
            if (targetVersion < 12)
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