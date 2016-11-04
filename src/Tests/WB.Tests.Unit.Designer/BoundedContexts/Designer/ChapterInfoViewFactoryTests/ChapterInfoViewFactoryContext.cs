using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ChapterInfoViewFactoryTests
{
    internal class ChapterInfoViewFactoryContext
    {
        protected static QuestionnaireDocument CreateQuestionnaireDocument(string questionnaireId, string chapterId)
        {
            return new QuestionnaireDocument()
            {
                PublicKey = Guid.Parse(questionnaireId),
                Children = new List<IComposite>() { new Group() { PublicKey = Guid.Parse(chapterId) } }.ToReadOnlyCollection()
            };
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithoutChapters(string questionnaireId)
        {
            return new QuestionnaireDocument()
            {
                PublicKey = Guid.Parse(questionnaireId)
            };
        }

        protected static ChapterInfoViewFactory CreateChapterInfoViewFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> repository = null)
        {
            return new ChapterInfoViewFactory(repository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>());
        }
    }
}