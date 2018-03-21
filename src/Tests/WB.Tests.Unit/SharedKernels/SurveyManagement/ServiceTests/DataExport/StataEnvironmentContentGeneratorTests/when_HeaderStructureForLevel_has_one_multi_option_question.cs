using System;
using System.Threading;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.StataEnvironmentContentGeneratorTests
{
    internal class when_HeaderStructureForLevel_has_one_multi_option_question : StataEnvironmentContentGeneratorTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var multiOptionQuestion =
                    Create.Entity.MultyOptionsQuestion(id: Guid.NewGuid(), variable: questionsVariableName,
                        options: new[] { Create.Entity.Option("1", "one"), Create.Entity.Option("3", "three") });
            multiOptionQuestion.QuestionText = questionsTitle;
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new[]
            {
                multiOptionQuestion
            });
            questionnaireDocument.Title = dataFileName;

            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1, null);
            var fileSystemAccessor = CreateFileSystemAccessor((c) => stataGeneratedContent = c);
             questionnaireExportStructure = new ExportViewFactory(fileSystemAccessor, 
                    new ExportQuestionService(),
                    Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>()) == questionnaire &&
                                                        _.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaireDocument),
                    new RosterStructureService(),
                    Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>())
                 .CreateQuestionnaireExportStructure(new QuestionnaireIdentity());

            stataEnvironmentContentService = CreateStataEnvironmentContentGenerator(fileSystemAccessor);
            BecauseOf();
        }

        public void BecauseOf() => stataEnvironmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, "", default(CancellationToken))/*.CreateContentOfAdditionalFile(oneQuestionHeaderStructureForLevel,dataFileName, contentFilePath)*/;

        [NUnit.Framework.Test] public void should_contain_stata_script_for_insheet_file () =>
            stataGeneratedContent.Should().Contain(string.Format("insheet using \"{0}.tab\", tab case\r\n", dataFileName));

        [NUnit.Framework.Test] public void should_contain_stata_variable_on_title_mapping_for_first_option () =>
           stataGeneratedContent.Should().Contain("label variable var1__1 `\"title1:one\"'");

        [NUnit.Framework.Test] public void should_contain_stata_variable_on_title_mapping_for_second_option () =>
            stataGeneratedContent.Should().Contain("label variable var1__3 `\"title1:three\"'");

        [NUnit.Framework.Test] public void should_not_contain_stata_variable_on_label_mapping_for_first_option () =>
           stataGeneratedContent.Should().NotContain("label values var1__1 lvar1__1");

        [NUnit.Framework.Test] public void should_not_contain_stata_variable_on_label_mapping_for_second_option () =>
           stataGeneratedContent.Should().NotContain("label values var1__3 lvar1__3");

        [NUnit.Framework.Test] public void should_not_contain_label_definition () =>
            stataGeneratedContent.Should().NotContain("label define lvar1");

        private static StataEnvironmentContentService stataEnvironmentContentService;
        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static string dataFileName = "data file name";

        private static string questionsVariableName = "var1";
        private static string questionsTitle = "title1";
        private static string stataGeneratedContent;
    }
}
