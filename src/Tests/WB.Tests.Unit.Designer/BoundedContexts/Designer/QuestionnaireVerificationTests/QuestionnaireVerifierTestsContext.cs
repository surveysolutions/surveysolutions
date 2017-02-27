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
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    [Subject(typeof(QuestionnaireVerifier))]
    internal class QuestionnaireVerifierTestsContext
    {
        protected static QuestionnaireVerifier CreateQuestionnaireVerifier(
            IExpressionProcessor expressionProcessor = null,
            ISubstitutionService substitutionService = null, 
            IKeywordsProvider keywordsProvider = null, 
            IExpressionProcessorGenerator expressionProcessorGenerator = null,
            IMacrosSubstitutionService macrosSubstitutionService = null,
            ILookupTableService lookupTableService = null,
            IAttachmentService attachmentService = null,
            ITopologicalSorter<string> topologicalSorter = null,
            IQuestionnaireTranslator questionnaireTranslator = null)
            => Create.QuestionnaireVerifier(
                expressionProcessor: expressionProcessor,
                substitutionService: substitutionService,
                keywordsProvider: keywordsProvider,
                expressionProcessorGenerator: expressionProcessorGenerator,
                macrosSubstitutionService: macrosSubstitutionService,
                lookupTableService: lookupTableService,
                attachmentService: attachmentService,
                topologicalSorter: topologicalSorter,
                questionnaireTranslator: questionnaireTranslator);

        protected static QuestionnaireDocument CreateQuestionnaireDocument(params IComposite[] questionnaireChildren)
        {
            return new QuestionnaireDocument
            {
                Children = questionnaireChildren.ToReadOnlyCollection(),
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
                        IsInteger = true
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
                        }.ToReadOnlyCollection()
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
                        IsInteger = true
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
                        }.ToReadOnlyCollection()
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
                        IsInteger = true
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
                        }.ToReadOnlyCollection()
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
                        IsInteger = true
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
                        }.ToReadOnlyCollection()
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
                        IsInteger = true
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
                        }.ToReadOnlyCollection()
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
                        Children = chapterChildren.ToReadOnlyCollection(),
                        IsRoster = false
                    }
                }.ToReadOnlyCollection()
            };
        }

        public static IExpressionProcessorGenerator CreateExpressionProcessorGenerator(ICodeGenerator codeGenerator = null, IDynamicCompiler dynamicCompiler = null)
        {
            var fileSystemAccessor = new FileSystemIOAccessor();

            const string pathToProfile = "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.5\\Profile\\Profile111";
            var referencesToAdd = new[] { "System.dll","System.Core.dll","System.Runtime.dll","System.Collections.dll","System.Linq.dll","System.Linq.Expressions.dll","System.Linq.Queryable.dll","mscorlib.dll","System.Runtime.Extensions.dll","System.Text.RegularExpressions.dll" };

            var settings = new List<IDynamicCompilerSettings>
            {
                Mock.Of<IDynamicCompilerSettings>(_ 
                    => _.PortableAssembliesPath == pathToProfile
                    && _.DefaultReferencedPortableAssemblies == referencesToAdd 
                    && _.Name == "profile111")
            };

            var defaultDynamicCompilerSettings = Mock.Of<ICompilerSettings>(_ => _.SettingsCollection == settings);

            return new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(),
                    Create.CodeGenerator(),
                    new DynamicCompilerSettingsProvider(defaultDynamicCompilerSettings, fileSystemAccessor));
        }

        

        public static QuestionnaireVerificationMessage FindWarningForEntityWithId(IEnumerable<QuestionnaireVerificationMessage> errors, string code, Guid entityId)
        {
            return errors.FirstOrDefault(message
                => message.MessageLevel == VerificationMessageLevel.Warning
                   && message.Code == code &&
                   message.References.First().Id == entityId);
        }
    }
}