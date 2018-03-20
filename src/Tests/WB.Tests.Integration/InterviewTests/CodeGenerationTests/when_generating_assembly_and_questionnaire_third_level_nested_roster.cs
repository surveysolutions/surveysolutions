using System;
using System.Collections.Generic;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_generating_assembly_and_questionnaire_third_level_nested_roster : CodeGenerationTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
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

                QuestionnaireDocument questionnaireDocument =
                    Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(Guid.Parse("31111111111111111111111111111113"),
                        Abc.Create.Entity.Group(children: new List<IComposite>
                        {
                            Abc.Create.Entity.NumericIntegerQuestion(id: level1QuestionId, variable: "num1"),
                            Abc.Create.Entity.NumericRoster(roster1, variable: "roster1", 
                                rosterSizeQuestionId: level1QuestionId, 
                                children: new IComposite[]{ 
                                    Abc.Create.Entity.NumericIntegerQuestion(level2QuestionId, variable: "num2"),
                                    Abc.Create.Entity.NumericRoster(roster2, "roster2", rosterSizeQuestionId: level2QuestionId) }),
                        }),
                        Abc.Create.Entity.Group(children: new List<IComposite>
                        {
                            Abc.Create.Entity.NumericRoster(roster3, variable:"roster3", 
                                rosterSizeQuestionId: level1QuestionId,
                                children: new []
                                {
                                    Abc.Create.Entity.NumericRoster(roster4, variable:"roster4", 
                                        rosterSizeQuestionId: level2QuestionId, 
                                        children: new IComposite[]
                                        {
                                            Abc.Create.Entity.NumericIntegerQuestion(level3QuestionId, variable: "num3"),
                                            Abc.Create.Entity.NumericRoster(roster5, variable:"roster5",  rosterSizeQuestionId: level3QuestionId)
                                        })
                                })
                        }));

                GenerationResult emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument, CreateQuestionnaireVersion(), out resultAssembly);

                return new InvokeResults
                {
                    Success = emitResult.Success
                };
            });

        [NUnit.Framework.Test] public void should_result_succeeded () =>
            results.Success.Should().Be(true);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public bool Success { get; set; }
        }
    }
}
