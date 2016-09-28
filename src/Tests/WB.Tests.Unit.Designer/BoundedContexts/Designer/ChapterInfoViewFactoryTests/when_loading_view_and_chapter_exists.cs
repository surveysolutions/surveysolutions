using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ChapterInfoViewFactoryTests
{
    internal class when_loading_view_and_chapter_exists : ChapterInfoViewFactoryContext
    {
        Establish context = () =>
        {
            var repositoryMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();

            repositoryMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(Create.QuestionnaireDocumentWithOneChapter(chapterId, 
                    Create.TextListQuestion(variable: "list"),
                    Create.FixedRoster(variable: "fixed_roster"),
                    Create.Variable(variableName: "variable")
                ));

            factory = CreateChapterInfoViewFactory(repository: repositoryMock.Object);
        };

        Because of = () =>
            view = factory.Load(questionnaireId, chapterId.FormatGuid());

        It should_find_chapter = () =>
            view.ShouldNotBeNull();

        It should_chapter_id_be_equal_chapterId = () =>
            view.Chapter.ItemId.ShouldEqual(chapterId.FormatGuid());

        It should_view_have_all_variabe_names_ = () =>
            view.VariableNames.Length.ShouldEqual(keywordsAndVariables.Length);

        It should_contain_all_variabe_names_ = () =>
            view.VariableNames.Select(x => x.Name).ShouldContain(keywordsAndVariables);

        private static NewChapterView view;
        private static ChapterInfoViewFactory factory;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static Guid chapterId = Guid.Parse("22222222222222222222222222222222");
        private static readonly string[] keywordsAndVariables =
        {
            "list",
            "fixed_roster",
            "variable",
            "self",
            "@optioncode",
            "@rowindex",
            "@rowname",
            "@rowcode"
        };
}
}
