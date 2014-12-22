using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class when_counting_questionnaires_which_are_not_migrated_to_csharp_and_4_questionnaires_have_csharp_flag_and_5_do_not_have : QuestionnaireInfoViewFactoryContext
    {
        Establish context = () =>
        {
            var documentReader = Setup.QueryableReadSideRepositoryReaderByQueryResultType<QuestionnaireDocument, int>(new[]
            {
                Create.QuestionnaireDocument(usesCSharp: true),
                Create.QuestionnaireDocument(usesCSharp: true),
                Create.QuestionnaireDocument(usesCSharp: true),
                Create.QuestionnaireDocument(usesCSharp: true),

                Create.QuestionnaireDocument(usesCSharp: false),
                Create.QuestionnaireDocument(usesCSharp: false),
                Create.QuestionnaireDocument(usesCSharp: false),
                Create.QuestionnaireDocument(usesCSharp: false),
                Create.QuestionnaireDocument(usesCSharp: false),
            });

            factory = CreateQuestionnaireInfoViewFactory(documentReader: documentReader);
        };

        Because of = () =>
            result = factory.CountQuestionnairesNotMigratedToCSharp();

        It should_return_5 = () =>
            result.ShouldEqual(5);

        private static QuestionnaireInfoViewFactory factory;
        private static int result;
    }
}