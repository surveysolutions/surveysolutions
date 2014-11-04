using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireInfoViewFactoryTests
{
    internal class when_getting_questionnaires_which_are_not_migrated_to_csharp_and_4_questionnaires_have_csharp_flag_and_5_do_not_have : QuestionnaireInfoViewFactoryContext
    {
        Establish context = () =>
        {
            var documentReader = Setup.QueryableReadSideRepositoryReaderByQueryResultType<QuestionnaireDocument, IEnumerable<Guid>>(new[]
            {
                Create.QuestionnaireDocument(id: Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"), usesCSharp: true),
                Create.QuestionnaireDocument(id: Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"), usesCSharp: true),
                Create.QuestionnaireDocument(id: Guid.Parse("cccccccccccccccccccccccccccccccc"), usesCSharp: true),
                Create.QuestionnaireDocument(id: Guid.Parse("dddddddddddddddddddddddddddddddd"), usesCSharp: true),

                Create.QuestionnaireDocument(id: Guid.Parse("11111111111111111111111111111111"), usesCSharp: false),
                Create.QuestionnaireDocument(id: Guid.Parse("22222222222222222222222222222222"), usesCSharp: false),
                Create.QuestionnaireDocument(id: Guid.Parse("33333333333333333333333333333333"), usesCSharp: false),
                Create.QuestionnaireDocument(id: Guid.Parse("44444444444444444444444444444444"), usesCSharp: false),
                Create.QuestionnaireDocument(id: Guid.Parse("55555555555555555555555555555555"), usesCSharp: false),
            });

            factory = CreateQuestionnaireInfoViewFactory(documentReader: documentReader);
        };

        Because of = () =>
            result = factory.GetQuestionnairesNotMigratedToCSharp();

        It should_return_only_ids_of_questionnaires_without_csharp_flag = () =>
            result.ShouldContainOnly(new []
            {
                Guid.Parse("11111111111111111111111111111111"),
                Guid.Parse("22222222222222222222222222222222"),
                Guid.Parse("33333333333333333333333333333333"),
                Guid.Parse("44444444444444444444444444444444"),
                Guid.Parse("55555555555555555555555555555555"),
            });

        private static QuestionnaireInfoViewFactory factory;
        private static IEnumerable<Guid> result;
    }
}