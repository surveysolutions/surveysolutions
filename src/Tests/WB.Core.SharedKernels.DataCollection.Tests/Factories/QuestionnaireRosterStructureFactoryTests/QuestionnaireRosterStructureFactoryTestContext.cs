using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Supervisor.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Tests.Factories.QuestionnaireRosterStructureFactoryTests
{
    [Subject(typeof(QuestionnaireRosterStructureFactory))]
    internal class QuestionnaireRosterStructureFactoryTestContext
    {
        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] children)
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

        protected static QuestionnaireRosterStructureFactory CreateQuestionnaireRosterStructureFactory()
        {
            return new QuestionnaireRosterStructureFactory();
        }
    }
}
