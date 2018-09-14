using System;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ChapterInfoViewFactoryTests
{
    internal class ChapterInfoViewFactoryContext
    {
        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithoutChapters(string questionnaireId)
        {
            return new QuestionnaireDocument
            {
                PublicKey = Guid.Parse(questionnaireId)
            };
        }

        protected static ChapterInfoViewFactory CreateChapterInfoViewFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> repository = null)
        {
            return new ChapterInfoViewFactory(
                repository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(),
                Create.QuestionTypeToCSharpTypeMapper());
        }
    }
}
