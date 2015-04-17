using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [Subject(typeof(QuestionnaireVerifier))]
    internal class QuestionnaireVerifierTestsContext
    {
        protected static QuestionnaireVerifier CreateQuestionnaireVerifier(IExpressionProcessor expressionProcessor = null,
            ISubstitutionService substitutionService = null, IKeywordsProvider keywordsProvider = null, IExpressionProcessorGenerator expressionProcessorGenerator = null)
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.MakeValidFileName(Moq.It.IsAny<string>())).Returns<string>(s => s);

            var questionnireExpressionProcessorGeneratorMock = new Mock<IExpressionProcessorGenerator>();
            string generationResult;
            questionnireExpressionProcessorGeneratorMock.Setup(
                _ => _.GenerateProcessorStateAssemblyForVersion(Moq.It.IsAny<QuestionnaireDocument>(),Moq.It.IsAny<ExpressionsEngineVersion>(), out generationResult))
                .Returns(new GenerationResult() {Success = true, Diagnostics = new List<GenerationDiagnostic>()});

            var substitutionServiceInstance = new SubstitutionService();

            return new QuestionnaireVerifier(expressionProcessor ?? new Mock<IExpressionProcessor>().Object, 
                fileSystemAccessorMock.Object,
                substitutionService ?? substitutionServiceInstance,
                keywordsProvider ?? new KeywordsProvider(substitutionServiceInstance),
                expressionProcessorGenerator ?? questionnireExpressionProcessorGeneratorMock.Object, new ExpressionsEngineVersionService());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(params IComposite[] questionnaireChildren)
        {
            return new QuestionnaireDocument
            {
                Children = questionnaireChildren.ToList(),
            };
        }

        protected static QuestionnaireDocument CreateQuestionnaireWithRosterWithConditionReferencingQuestionInsideItself(Guid questionIdFromRoster,
            Guid rosterWithCustomValidation)
        {
            var rosterQuestionId = Guid.Parse("a3333333333333333333333333333333");
            var questionnaire = CreateQuestionnaireDocument(new IComposite[]
                {
                    new NumericQuestion
                    {
                        PublicKey = rosterQuestionId,
                        StataExportCaption = "var1",
                        IsInteger = true, 
                        MaxValue = 5
                    },
                    new Group
                    {
                        PublicKey = rosterWithCustomValidation,
                        IsRoster = true,
                        VariableName = "a",
                        RosterSizeQuestionId = rosterQuestionId,
                        ConditionExpression = "some random expression",
                        Children = new List<IComposite>
                        {
                            new NumericQuestion
                            {
                                StataExportCaption = "var2",
                                PublicKey = questionIdFromRoster
                            }
                        }
                    }
                });

            return questionnaire;
        }

        protected static QuestionnaireDocument CreateQuestionnaireWithTwoRosterWithConditionInLastOneRosterReferencingQuestionFromFirstOne(
            Guid questionIdFromOtherRosterWithSameLevel, Guid rosterWithCustomCondition)
        {
            var numId = Guid.Parse("a3333333333333333333333333333333");

            var questionnaire = CreateQuestionnaireDocument(new IComposite[]
                {
                    new NumericQuestion
                    {
                        PublicKey = numId, 
                        StataExportCaption = "var1",
                        IsInteger = true, 
                        MaxValue = 5
                    },
                    new Group
                    {
                        PublicKey = Guid.Parse("13333333333333333333333333333333"),
                        IsRoster = true,
                        VariableName = "a",
                        RosterSizeQuestionId = numId,
                        Children = new List<IComposite>
                        {
                            new NumericQuestion
                            {
                                StataExportCaption = "var2",
                                PublicKey = questionIdFromOtherRosterWithSameLevel
                            }
                        }
                    },
                    new Group
                    {
                        IsRoster = true,
                        VariableName = "b",
                        RosterSizeQuestionId = Guid.Parse("a3333333333333333333333333333333"),
                        PublicKey = rosterWithCustomCondition,
                        ConditionExpression = "some random expression"
                    }
                });

            return questionnaire;
        }

        protected static QuestionnaireDocument CreateQuestionnaireWithRosterAndGroupAfterWithConditionReferencingQuestionInRoster(Guid underDeeperRosterLevelQuestionId, Guid questionWithCustomValidation)
        {
            var rosterGroupId = Guid.Parse("13333333333333333333333333333333");
            var rosterQuestionId = Guid.Parse("13333333333333333333333333333333");
            var questionnaire = CreateQuestionnaireDocument(new IComposite[]
                {
                    new NumericQuestion
                    {
                        PublicKey = rosterGroupId, 
                        StataExportCaption = "var1",
                        IsInteger = true, 
                        MaxValue = 5
                    },
                    new Group
                    {
                        PublicKey = rosterGroupId,
                        IsRoster = true,
                        VariableName = "a",
                        RosterSizeQuestionId = rosterQuestionId,
                        Children = new List<IComposite>
                        {
                            new NumericQuestion
                            {
                                StataExportCaption = "var2",
                                PublicKey = underDeeperRosterLevelQuestionId
                            }
                        }
                    },
                    new Group
                    {
                        PublicKey = questionWithCustomValidation,
                        ConditionExpression = "some random expression"
                    }
                });

            return questionnaire;
        }

        protected static QuestionnaireDocument CreateQuestionnaireWithTwoRosterWithSomeConditionInOneRoster(Guid underDeeperRosterLevelQuestionId, Guid groupWithCustomValidation)
        {
            Guid numKey1 = Guid.Parse("a3333333333333333333333333333333");

            var questionnaire = CreateQuestionnaireDocument(new IComposite[]
                {
                    new NumericQuestion
                    {
                        PublicKey = numKey1, 
                        StataExportCaption = numKey1.ToString(),
                        IsInteger = true, 
                        MaxValue = 5
                    },
                    new Group
                    {
                        PublicKey = Guid.Parse("13333333333333333333333333333333"),
                        IsRoster = true,
                        VariableName = "a",
                        RosterSizeQuestionId = numKey1,
                        Children = new List<IComposite>
                        {
                            new NumericQuestion
                            {
                                StataExportCaption = underDeeperRosterLevelQuestionId.ToString(),
                                PublicKey = underDeeperRosterLevelQuestionId
                            }
                        }
                    },
                    new Group
                    {
                        IsRoster = true,
                        VariableName = "b",
                        RosterSizeQuestionId = Guid.Parse("a3333333333333333333333333333333"),
                        PublicKey = groupWithCustomValidation,
                        ConditionExpression = "some random expression"
                    }
                });

            return questionnaire;
        }


        protected static QuestionnaireDocument CreateQuestionnaireWithRosterAndQuestionAfterWithConditionReferencingQuestionInRoster(Guid underDeeperRosterLevelQuestionId, Guid questionWithCustomValidation)
        {
            var rosterGroupId = Guid.Parse("13333333333333333333333333333333");
            var rosterQuestionId = Guid.Parse("13333333333333333333333333333333");
            var questionnaire = CreateQuestionnaireDocument(new IComposite[]
                {
                    new NumericQuestion
                    {
                        PublicKey = rosterGroupId, 
                        StataExportCaption = "var1",
                        IsInteger = true, 
                        MaxValue = 5
                    },
                    new Group
                    {
                        PublicKey = rosterGroupId,
                        IsRoster = true,
                        RosterSizeQuestionId = rosterQuestionId,
                        VariableName = "a",
                        Children = new List<IComposite>
                        {
                            new NumericQuestion
                            {
                                StataExportCaption = "var2",
                                PublicKey = underDeeperRosterLevelQuestionId
                            }
                        }
                    },
                    new SingleQuestion
                    {
                        StataExportCaption = "var3",
                        PublicKey = questionWithCustomValidation,
                        ConditionExpression = "some random expression",
                        Answers = { new Answer(){ AnswerValue = "1", AnswerText = "opt 1" }, new Answer(){ AnswerValue = "2", AnswerText = "opt 2" }}
                    }
                });

            return questionnaire;
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren.ToList(),
                        IsRoster = false
                    }
                }
            };
        }

        public static IExpressionProcessorGenerator CreateExpressionProcessorGenerator(ICodeGenerator codeGenerator = null, IDynamicCompiler dynamicCompiler = null)
        {
            var questionnaireVersionProvider = new ExpressionsEngineVersionService();
            return
                new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(
                        new DefaultDynamicCompillerSettings()
                        {
                            PortableAssembliesPath =
                                "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.5\\Profile\\Profile111",
                            DefaultReferencedPortableAssemblies = new[] { "System.dll", "System.Core.dll", "mscorlib.dll", "System.Runtime.dll", 
                                "System.Collections.dll", "System.Linq.dll" }
                        }, new FileSystemIOAccessor()),
                    new CodeGenerator(questionnaireVersionProvider), questionnaireVersionProvider);
        }
    }
}