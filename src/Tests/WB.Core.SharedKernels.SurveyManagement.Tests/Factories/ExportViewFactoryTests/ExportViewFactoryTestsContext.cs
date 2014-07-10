using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Factories.ExportViewFactoryTests
{
    [Subject(typeof(ExportViewFactory))]
    internal class ExportViewFactoryTestsContext
    {
        protected static ExportViewFactory CreateExportViewFactory(
            IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory = null)
        {
            return new ExportViewFactory(new ReferenceInfoForLinkedQuestionsFactory(), questionnaireRosterStructureFactory ?? new QuestionnaireRosterStructureFactory(), Mock.Of<IFileSystemAccessor>());
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
