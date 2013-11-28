using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views.ExportedHeaderCollectionTests
{
    [Subject(typeof(ExportedHeaderCollection))]
    internal class ExportedHeaderCollectionTestsContext
    {
        protected static ExportedHeaderCollection CreateExportedHeaderCollection(ReferenceInfoForLinkedQuestions questionnaireReferences, QuestionnaireDocument document)
        {
            return new ExportedHeaderCollection(questionnaireReferences, document);
        }

        protected static ReferenceInfoForLinkedQuestions CreateReferenceInfoForLinkedQuestionsWithOneLink(Guid linkedQuestionId, Guid scopeId, Guid referencedQuestionId)
        {
            return new ReferenceInfoForLinkedQuestions(Guid.NewGuid(), 1, new Dictionary<Guid, ReferenceInfoByQuestion> { { linkedQuestionId, new ReferenceInfoByQuestion(scopeId, referencedQuestionId) } });
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
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
        }
    }
}
