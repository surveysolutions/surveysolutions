using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Supervisor.Implementation.Factories;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views.ExportedHeaderCollectionTests
{
    [Subject(typeof(HeaderStructureForLevel))]
    internal class QuestionnaireExportStructureTestsContext
    {
        protected static QuestionnaireExportStructure CreateQuestionnaireExportStructure(QuestionnaireDocument questionnaire)
        {
            var exportViewFactory = new ExportViewFactory();
            return exportViewFactory.CreateQuestionnaireExportStructure(questionnaire, 1);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            var questionnaireDocument= new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren.ToList(),
                    }
                }
            };
            questionnaireDocument.ConnectChildrenWithParent();
            return questionnaireDocument;
        }
    }
}
