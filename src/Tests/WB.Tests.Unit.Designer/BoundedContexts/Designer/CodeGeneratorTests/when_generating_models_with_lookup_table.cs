using System;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Services;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generating_models_with_lookup_table : CodeGeneratorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            

            var lookupTableContent = Create.LookupTableContent(new[] { "min", "max" },
                Create.LookupTableRow(1, new decimal?[] { 1.15m, 10 }),
                Create.LookupTableRow(2, new decimal?[] { 1, 10 }),
                Create.LookupTableRow(3, new decimal?[] { 1, 10 })
                );

            var lookupTableServiceMock = new Mock<ILookupTableService>();
            lookupTableServiceMock
                .Setup(x => x.GetLookupTableContent(questionnaireId, lookupId))
                .Returns(lookupTableContent);

            var assetsTitles = new[]
            {
                Create.FixedRosterTitle(1, "TV"),
                Create.FixedRosterTitle(2, "Microwave"),
                Create.FixedRosterTitle(3, "Cleaner")
            };

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(id: questionA, variable: "a", validationExpression: "a > price[1].min && a < price[1].max"),
                    Create.Roster(rosterId, variable: "assets", rosterType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: assetsTitles,
                        children: new []
                        {
                            Create.NumericRealQuestion(id: questionB, variable: "p", validationExpression: "p.InRange(price[@rowcode].min, price[@rowcode].max)")
                        })
                })
            });

            questionnaire.LookupTables.Add(lookupId, Create.LookupTable("price"));

            templateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory(lookupTableService: lookupTableServiceMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            model = templateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        [NUnit.Framework.Test] public void should_generate_model_for_1_lookup_table () =>
            model.LookupTables.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_generate_model_with_TableName_price () =>
            model.LookupTables[0].TableName.Should().Be("price");

        [NUnit.Framework.Test] public void should_generate_model_with_TableNameField_price () =>
            model.LookupTables[0].TableNameField.Should().Be("@__price");

        [NUnit.Framework.Test] public void should_generate_model_with_TypeName_Price () =>
            model.LookupTables[0].TypeName.Should().Be("@Lookup__Price");

        [NUnit.Framework.Test] public void should_generate_model_with_VariableNames__min__max () =>
            model.LookupTables[0].VariableNames.Should().BeEquivalentTo("min", "max");

        private static Version version = new Version(11, 0, 0);
        private static QuestionnaireExpressionStateModelFactory templateModelFactory;
        private static QuestionnaireExpressionStateModel model;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
        private static readonly Guid questionA = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static readonly Guid questionB = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        private static readonly Guid rosterId = Guid.Parse("cccccccccccccccccccccccccccccccc");
        private static readonly Guid lookupId = Guid.Parse("dddddddddddddddddddddddddddddddd");
    }
}
