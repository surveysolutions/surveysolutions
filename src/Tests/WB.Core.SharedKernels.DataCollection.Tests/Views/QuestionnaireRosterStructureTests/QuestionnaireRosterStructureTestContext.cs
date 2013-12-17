using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.DataCollection.Tests.Views.QuestionnaireRosterStructureTests
{
    internal class QuestionnaireRosterStructureTestContext
    {
        protected static QuestionnaireDocument CreateQuestionnaireDocument(params IComposite[] children)
        {
            var result= new QuestionnaireDocument();
            var chapter = new Group("Chapter");
            result.Children.Add(chapter);

            foreach (var child in children)
            {
                chapter.Children.Add(child);
            }

            return result;
        }
    }
}
