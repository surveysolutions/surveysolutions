using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGenerationTests
{
    [Ignore("Failed test should be fixed in KP-5380")]
    internal class when_generating_assembly_and_questionnaire_third_level_nested_roster : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                string resultAssembly;

                AssemblyContext.SetupServiceLocator();

                var expressionProcessorGenerator = CreateExpressionProcessorGenerator();

                var level1QuestionId = Guid.Parse("11111111111111111111111111111111");
                var level2QuestionId = Guid.Parse("22222222222222222222222222222222");
                var level3QuestionId = Guid.Parse("33333333333333333333333333333333");
                var roster1 = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var roster2 = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var roster3 = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var roster4 = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var roster5 = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
                var roster6 = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

                QuestionnaireDocument questionnaireDocument =
                    Create.QuestionnaireDocument(
                        Guid.Parse("31111111111111111111111111111113"),
                        Create.Chapter(children: new List<IComposite>
                                {
                                    Create.NumericIntegerQuestion(id: level1QuestionId),
                                    Create.NumericRoster(roster1, "roster1", 
                                        rosterSizeQuestionId: level1QuestionId, 
                                        children: new IComposite[]{ 
                                            Create.NumericIntegerQuestion(level2QuestionId),
                                            Create.NumericRoster(roster3, "roster3", rosterSizeQuestionId: level2QuestionId) }),
                                }),
                        Create.Chapter(children: new List<IComposite>
                                {
                                    Create.NumericRoster(roster2, "roster2", 
                                        rosterSizeQuestionId: level1QuestionId,
                                        children: new []
                                                  {
                                                      Create.NumericRoster(roster4, "roster4", 
                                                        rosterSizeQuestionId: level2QuestionId, 
                                                        children: new IComposite[]
                                                                  {
                                                                        Create.NumericIntegerQuestion(level3QuestionId),
                                                                        Create.NumericRoster(roster5, "roster5",  rosterSizeQuestionId: level3QuestionId)
                                                                  })
                                                  })
                                }));

                GenerationResult emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument, CreateQuestionnaireVersion(), out resultAssembly);

                return new InvokeResults
                       {
                           Success = emitResult.Success
                       };
            });

        It should_result_succeeded = () =>
            results.Success.ShouldEqual(true);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static AppDomainContext appDomainContext;
        private static InvokeResults results;
        private static string[] namesToCheck = new[] { "parent", "conditionExpressions" };


        [Serializable]
        internal class InvokeResults
        {
            public bool Success { get; set; }
        }
    }
}