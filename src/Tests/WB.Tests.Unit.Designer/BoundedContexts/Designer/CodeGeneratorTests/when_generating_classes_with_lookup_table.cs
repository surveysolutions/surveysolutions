using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_generating_classes_with_lookup_table : CodeGeneratorTestsContext
    {
        Establish context = () =>
        {
            AssemblyContext.SetupServiceLocator();

            var lookupTableContent = Create.LookupTableContent(new[] { "min", "max" },
                Create.LookupTableRow(1, new decimal?[] { 1.15m, 10 }),
                Create.LookupTableRow(2, new decimal?[] { 1, 10 }),
                Create.LookupTableRow(3, new decimal?[] { 1, 10 })
                );

            var lookupTableServiceMock = new Mock<ILookupTableService>();
            lookupTableServiceMock
                .Setup(x => x.GetLookupTableContent(questionnaireId, lookupId))
                .Returns(lookupTableContent);

            Setup.InstanceToMockedServiceLocator<ILookupTableService>(lookupTableServiceMock.Object);

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
                }),
            });

            questionnaire.LookupTables.Add(lookupId, Create.LookupTable("price"));

            generator = Create.CodeGenerator();
        };

        Because of = () =>
            generatedClassContent = generator.Generate(questionnaire, version);

        It should_generate_class_for_lookup_tables_with_correct_key = () =>
            generatedClassContent.Keys.ShouldContain(lookupTableClassName);

        It should_generate_class_for_lookup_tables = () =>
            generatedClassContent[lookupTableClassName].ShouldContain("public static class LookupTables");

        It should_generate_class_for_Price = () =>
            generatedClassContent[lookupTableClassName].ShouldContain("public class @Lookup__Price");

        It should_generate_class_constructor_for_Price = () =>
            generatedClassContent[lookupTableClassName].ShouldContain("public @Lookup__Price(decimal rowcode, double? min,double? max)");

        It should_generate_lookup_table_price = () =>
            generatedClassContent[lookupTableClassName].ShouldContain("public static Dictionary<decimal, @Lookup__Price> price");

        It should_generate_lookup_table_field_price = () =>
            generatedClassContent[lookupTableClassName].ShouldContain("private static readonly Dictionary<decimal, @Lookup__Price> @__price = @Lookup__Price_Generator.GetTable();");


        private static int version = 11;
        private static CodeGenerator generator;
        private static Dictionary<string, string> generatedClassContent;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
        private static readonly Guid questionA = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static readonly Guid questionB = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        private static readonly Guid rosterId = Guid.Parse("cccccccccccccccccccccccccccccccc");
        private static readonly Guid lookupId = Guid.Parse("dddddddddddddddddddddddddddddddd");
        private static readonly string lookupTableClassName = new ExpressionLocation(ExpressionLocationItemType.LookupTable).ToString();
    }
}