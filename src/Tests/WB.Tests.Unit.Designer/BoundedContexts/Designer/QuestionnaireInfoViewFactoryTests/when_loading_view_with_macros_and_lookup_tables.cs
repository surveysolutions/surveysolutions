using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Documents;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoViewFactoryTests
{
    internal class when_loading_view_with_macros_and_lookup_tables : QuestionnaireInfoViewFactoryContext
    {
        [NUnit.Framework.Test]
        public void should_find_questionnaire()
        {
            var questionnaireDocument = CreateQuestionnaireDocument(questionnaireId.ToString(), questionnaireTitle);
            questionnaireDocument.Macros = macros;
            questionnaireDocument.LookupTables = lookupTables;

            var dbContext = Create.InMemoryDbContext();
            dbContext.Questionnaires.Add(Create.QuestionnaireListViewItem(id: questionnaireId.QuestionnaireId));
            dbContext.SaveChanges();

            var repositoryMock = new Mock<IDesignerQuestionnaireStorage>();
            repositoryMock
                .Setup(x => x.Get(questionnaireId))
                .Returns(questionnaireDocument);

            factory = CreateQuestionnaireInfoViewFactory(dbContext: dbContext, repository: repositoryMock.Object);
            BecauseOf();

            view.Should().NotBeNull();
            view.Macros.Select(x => x.ItemId).Should().BeEquivalentTo(macro1Id.FormatGuid(), macro2Id.FormatGuid(), macro3Id.FormatGuid(), macro4Id.FormatGuid());
            view.LookupTables.Select(x => x.ItemId).Should().BeEquivalentTo(lookupTable1Id.FormatGuid(), lookupTable2Id.FormatGuid());

            view.Macros.ElementAt(0).ItemId.Should().Be(macro4Id.FormatGuid());
            view.Macros.ElementAt(0).Name.Should().Be(macros[macro4Id].Name);
            view.Macros.ElementAt(0).Content.Should().Be(macros[macro4Id].Content);
            view.Macros.ElementAt(0).Description.Should().Be(macros[macro4Id].Description);

            view.Macros.ElementAt(1).ItemId.Should().Be(macro1Id.FormatGuid());
            view.Macros.ElementAt(1).Name.Should().Be(macros[macro1Id].Name);
            view.Macros.ElementAt(1).Content.Should().Be(macros[macro1Id].Content);
            view.Macros.ElementAt(1).Description.Should().Be(macros[macro1Id].Description);

            view.Macros.ElementAt(2).ItemId.Should().Be(macro2Id.FormatGuid());
            view.Macros.ElementAt(2).Name.Should().Be(macros[macro2Id].Name);
            view.Macros.ElementAt(2).Content.Should().Be(macros[macro2Id].Content);
            view.Macros.ElementAt(2).Description.Should().Be(macros[macro2Id].Description);

            view.Macros.ElementAt(3).ItemId.Should().Be(macro3Id.FormatGuid());
            view.Macros.ElementAt(3).Name.Should().Be(macros[macro3Id].Name);
            view.Macros.ElementAt(3).Content.Should().Be(macros[macro3Id].Content);
            view.Macros.ElementAt(3).Description.Should().Be(macros[macro3Id].Description);
        }

        private void BecauseOf() =>
            view = factory.Load(questionnaireId, userId);

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
            {macro1Id, Create.Macro("2. second", "content2", "description2")},
            {macro2Id, Create.Macro("3. third", "content3", "description3")},
            {macro3Id, Create.Macro("4. fourth", "content4", "description4")},
            {macro4Id, Create.Macro("1. first", "content1", "description1")}
        };

        private static Dictionary<Guid, LookupTable> lookupTables = new Dictionary<Guid, LookupTable>
        {
            {lookupTable1Id, Create.LookupTable("table1")},
            {lookupTable2Id, Create.LookupTable("table2")}
        };
        
        private static string questionnaireTitle = "questionnaire title";
        private static Guid userId = Guid.Parse("22222222222222222222222222222222");

    }
}
