using System;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
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

        protected static ChapterInfoViewFactory CreateChapterInfoViewFactory(IDesignerQuestionnaireStorage repository = null)
        {
            return new ChapterInfoViewFactory(
                repository ?? Mock.Of<IDesignerQuestionnaireStorage>(),
                Create.QuestionTypeToCSharpTypeMapper());
        }
    }
}
