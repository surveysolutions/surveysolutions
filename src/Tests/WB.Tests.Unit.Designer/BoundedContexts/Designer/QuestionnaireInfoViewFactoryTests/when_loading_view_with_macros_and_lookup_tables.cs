using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class when_loading_view_with_macros_and_lookup_tables : QuestionnaireInfoViewFactoryContext
    {
        Establish context = () =>
        {
            var questionnaireDocument = CreateQuestionnaireDocument(questionnaireId, questionnaireTitle);
            questionnaireDocument.Macros = macros;
            questionnaireDocument.LookupTables = lookupTables;

            var repositoryMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            repositoryMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireDocument);

            factory = CreateQuestionnaireInfoViewFactory(repository: repositoryMock.Object);
        };

        Because of = () =>
            view = factory.Load(questionnaireId, userId);

        It should_find_questionnaire = () =>
            view.ShouldNotBeNull();

        It should_contains_all_macroses_from_questionnaire = () =>
            view.Macros.Select(x => x.ItemId).ShouldContainOnly(macro1Id.FormatGuid(), macro2Id.FormatGuid(), macro3Id.FormatGuid(), macro4Id.FormatGuid());

        It should_contains_all_lookups_from_questionnaire = () =>
            view.LookupTables.Select(x => x.ItemId).ShouldContainOnly(lookupTable1Id.FormatGuid(), lookupTable2Id.FormatGuid());

        It should_first_element_match_macro_with_id_macro4Id = () =>
        {
            view.Macros.ElementAt(0).ItemId.ShouldEqual(macro4Id.FormatGuid());
            view.Macros.ElementAt(0).Name.ShouldEqual(macros[macro4Id].Name);
            view.Macros.ElementAt(0).Content.ShouldEqual(macros[macro4Id].Content);
            view.Macros.ElementAt(0).Description.ShouldEqual(macros[macro4Id].Description);
        };

        It should_second_element_match_macro_with_id_macro1Id = () =>
        {
            view.Macros.ElementAt(1).ItemId.ShouldEqual(macro1Id.FormatGuid());
            view.Macros.ElementAt(1).Name.ShouldEqual(macros[macro1Id].Name);
            view.Macros.ElementAt(1).Content.ShouldEqual(macros[macro1Id].Content);
            view.Macros.ElementAt(1).Description.ShouldEqual(macros[macro1Id].Description);
        };

        It should_third_element_match_macro_with_id_macro2Id = () =>
        {
            view.Macros.ElementAt(2).ItemId.ShouldEqual(macro2Id.FormatGuid());
            view.Macros.ElementAt(2).Name.ShouldEqual(macros[macro2Id].Name);
            view.Macros.ElementAt(2).Content.ShouldEqual(macros[macro2Id].Content);
            view.Macros.ElementAt(2).Description.ShouldEqual(macros[macro2Id].Description);
        };

        It should_fourth_element_match_macro_with_id_macro3Id = () =>
        {
            view.Macros.ElementAt(3).ItemId.ShouldEqual(macro3Id.FormatGuid());
            view.Macros.ElementAt(3).Name.ShouldEqual(macros[macro3Id].Name);
            view.Macros.ElementAt(3).Content.ShouldEqual(macros[macro3Id].Content);
            view.Macros.ElementAt(3).Description.ShouldEqual(macros[macro3Id].Description);
        };

        private static QuestionnaireInfoView view;
        private static QuestionnaireInfoViewFactory factory;
        private static Guid macro1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid macro2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid macro3Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid macro4Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

        private static Guid lookupTable1Id = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid lookupTable2Id = Guid.Parse("2BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        private static Dictionary<Guid, Macro> macros = new Dictionary<Guid, Macro>
        {
            { macro1Id, Create.Macro("2. second", "content2", "description2") },
            { macro2Id, Create.Macro("3. third", "content3", "description3") },
            { macro3Id, Create.Macro("4. fourth", "content4", "description4") },
            { macro4Id, Create.Macro("1. first", "content1", "description1") }
        };

        private static Dictionary<Guid, LookupTable> lookupTables = new Dictionary<Guid, LookupTable>
        {
            { lookupTable1Id, Create.LookupTable("table1")},
            { lookupTable2Id, Create.LookupTable("table2")}
        };
        private static string questionnaireId = "11111111111111111111111111111111";
        private static string questionnaireTitle = "questionnaire title";
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");
      
    }
}