using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Machine.Specifications.Annotations;
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
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGenerationTests
{
    public class CodeGenerationTestsContext
    {
        public static QuestionnaireDocument CreateQuestionnaireForGeneration(Guid? questionnaireId = null)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId ?? Guid.NewGuid() };

            Guid chapterId = Guid.Parse("23232323232323232323232323232323");
            Guid questionId = Guid.Parse("23232323232323232323232323232311");

            questionnaireDocument.AddChapter(chapterId);

            questionnaireDocument.Add(new NumericQuestion()
            {
                PublicKey = questionId,
                StataExportCaption = "persons_n",
                IsInteger = true
            }, chapterId, null);

            var rosterId = Guid.Parse("23232323232323232323232323232322");
            questionnaireDocument.Add(new Group()
            {
                PublicKey = rosterId,
                IsRoster = true,
                RosterSizeQuestionId = questionId,
                VariableName = "r1"
            }, chapterId, null);

            var rosterId1 = Guid.Parse("23232323232323232323232323232388");
            questionnaireDocument.Add(new Group()
            {
                PublicKey = rosterId1,
                IsRoster = true,
                RosterSizeQuestionId = questionId,
                VariableName = "r2"
            }, chapterId, null);

            questionnaireDocument.Add(new NumericQuestion()
            {
                PublicKey = Guid.Parse("23232323232323232323232323232399"),
                StataExportCaption = "q_22",
                IsInteger = true,
                QuestionType = QuestionType.Numeric
            }, rosterId1, null);


            Guid pets_questionId = Guid.Parse("23232323232323232323232323232317");
            questionnaireDocument.Add(new NumericQuestion()
            {
                PublicKey = pets_questionId,
                StataExportCaption = "pets_n",
                IsInteger = true,
                Mandatory = true,
                QuestionType = QuestionType.Numeric
            }, rosterId, null);

            var groupId = Guid.Parse("12345678912345678912345678912345");
            questionnaireDocument.Add(new Group()
            {
                PublicKey = groupId,
                IsRoster = false,
                ConditionExpression = "pets_n > 0"
            }, rosterId, null);

            questionnaireDocument.Add(new TextQuestion()
            {
                PublicKey = Guid.Parse("12345678912345678912345678912340"),
                StataExportCaption = "pets_text",
                ConditionExpression = "pets_n > 0",
                ValidationExpression = "pets_n == 0",
                Mandatory = true,
                QuestionType = QuestionType.Text
            }, groupId, null);

            questionnaireDocument.Add(new Group()
            {
                PublicKey = Guid.Parse("23232323232323232323232323232324"),
                IsRoster = true,
                RosterSizeQuestionId = pets_questionId,
                ConditionExpression = "pets_text.Length > 0"
            }, rosterId, null);

            return questionnaireDocument;
        }


        public static QuestionnaireDocument CreateQuestionnairDocumenteWithOneNumericIntegerQuestionWithValidationAndTwoRosters(
            Guid questionnaireId, Guid questionId)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            Guid chapterId = Guid.Parse("23232323232323232323232323232323");
            Guid roster1Id = Guid.Parse("23232323232323232323232323232311");
            Guid roster2Id = Guid.Parse("23232323232323232323232323232111");

            Guid question1Id = Guid.Parse("13232323232323232323232323232111");

            questionnaireDocument.AddChapter(chapterId);

            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = questionId,
                StataExportCaption = "test",
                IsInteger = true,
                ValidationExpression = "test > 3"
            }, chapterId, null);

            questionnaireDocument.Add(new Group()
            {
                PublicKey = roster1Id,
                VariableName = "roster1",
                IsRoster = true,
                RosterSizeQuestionId = questionId,
                RosterSizeSource = RosterSizeSourceType.Question
            }, chapterId, null);

            questionnaireDocument.Add(new Group()
            {
                PublicKey = roster2Id,
                VariableName = "roster2",
                IsRoster = true,
                RosterSizeQuestionId = questionId,
                RosterSizeSource = RosterSizeSourceType.Question
            }, chapterId, null);

            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = question1Id,
                StataExportCaption = "test_in_r",
                IsInteger = true,
                ValidationExpression = "test > 3"
            }, roster2Id, null);

            return questionnaireDocument;
        }

        public static QuestionnaireDocument CreateQuestionnaireWithCategoricalMultiLinkedMandatoryQuestion()
        {
            var linkedToQuestionId = new Guid("11111111111111111111111111111111");
            return new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    new Group("Chapter")
                    {
                        Children = new List<IComposite>()
                        {
                            new Group("Roster")
                            {
                                IsRoster = true,
                                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                                RosterFixedTitles = new[] {"Roster row 1", "Roster row 2"},
                                Children = new List<IComposite>()
                                {
                                    new TextQuestion("Text") {PublicKey = linkedToQuestionId}
                                }
                            },
                            new MultyOptionsQuestion("Categrical multi linked mandatory")
                            {
                                QuestionType = QuestionType.MultyOption,
                                Mandatory = true,
                                LinkedToQuestionId = linkedToQuestionId
                            }
                        }
                    }
                }
            };
        }

        public static QuestionnaireDocument CreateQuestionnaireWithRosterAndNamedTextQuestions(string[] varNames)
        {

            var roster = new Group("Roster")
            {
                IsRoster = true,
                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                RosterFixedTitles = new[] {"Roster row 1", "Roster row 2"}
            };

            foreach (var varName in varNames)
            {
                roster.Children.Add(new TextQuestion(varName)
                                    {
                                        PublicKey = Guid.NewGuid(),
                                        StataExportCaption = varName
                                    });
            }

            return new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    new Group("Chapter")
                    {
                        Children = new List<IComposite>()
                        {
                            roster
                        }
                    }
                }
            };
        }

        public static Version CreateQuestionnaireVersion()
        {
            return new ExpressionsEngineVersionService().GetLatestSupportedVersion();
        }

        public static QuestionnaireDocument CreateQuestionnaireWithQuestionAndRosterWithQuestionWithInvalidExpressions(Guid questionId, Guid questionInRosterId)
        {
            return new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    new Group("Chapter")
                    {
                        Children = new List<IComposite>()
                        {
                            new Group("Roster")
                            {
                                IsRoster = true,
                                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                                RosterFixedTitles = new[] {"Roster row 1", "Roster row 2"},
                                Children = new List<IComposite>()
                                {
                                    new TextQuestion("Text")
                                    {
                                        PublicKey = questionInRosterId,
                                        ValidationExpression = "if"
                                    }
                                }
                            },
                            new TextQuestion("Text")
                            {
                                QuestionType = QuestionType.Text,
                                PublicKey = questionId,
                                ConditionExpression = "bool"
                            }
                        }
                    }
                }
            };
        }

        public static QuestionnaireDocument CreateQuestionnaireDocumenteWithOneNumericIntegerQuestionAndRosters(Guid questionnaireId,
            Guid questionId, Guid rosterId)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            Guid chapterId = Guid.Parse("23232323232323232323232323232323");
            Guid question1Id = Guid.Parse("13232323232323232323232323232111");

            questionnaireDocument.AddChapter(chapterId);

            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = questionId,
                StataExportCaption = "test",
                IsInteger = true,
                ValidationExpression = "test >= 1"
            }, chapterId, null);

            questionnaireDocument.Add(new Group()
            {
                PublicKey = rosterId,
                VariableName = "roster1",
                IsRoster = true,
                RosterSizeQuestionId = questionId,
                RosterSizeSource = RosterSizeSourceType.Question
            }, chapterId, null);


            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = question1Id,
                StataExportCaption = "test_in_roster",
                IsInteger = true,
                ValidationExpression = "test >= 1"
            }, rosterId, null);

            return questionnaireDocument;
        }

        public static QuestionnaireDocument CreateQuestionnairDocumenteWithTwoNumericIntegerQuestionAndConditionalGroup(
            Guid questionnaireId, Guid questionId, Guid group1Id)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            //Guid chapterId = Guid.Parse("23232323232323232323232323232323");
            Guid question1Id = Guid.Parse("23232323232323232323232323232311");

            //questionnaireDocument.AddChapter(chapterId);

            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = questionId,
                StataExportCaption = "q1",
                IsInteger = true,
            }, questionnaireId, null);

            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = question1Id,
                StataExportCaption = "q2",
                IsInteger = true,
                ConditionExpression = "q1 > 3"
            }, questionnaireId, null);

            questionnaireDocument.Add(new Group()
            {
                PublicKey = group1Id,
                ConditionExpression = "q1 > 5"
            }, questionnaireId, null);


            return questionnaireDocument;
        }

        public static QuestionnaireDocument CreateQuestionnairDocumenteHavingRosterWithConditions(Guid questionnaireId, Guid questionId,
            Guid group1Id)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = questionId,
                StataExportCaption = "q1",
                IsInteger = true,
            }, questionnaireId, null);


            questionnaireDocument.Add(new Group()
            {
                PublicKey = group1Id,
                IsRoster = true,
                RosterSizeQuestionId = questionId,
                ConditionExpression = "q1 > 5"
            }, questionnaireId, null);

            return questionnaireDocument;
        }

        public static QuestionnaireDocument CreateQuestionnaireDocumenteHavingMandatoryQuestions(Guid questionnaireId, Guid questionId,
            Guid question1Id, Guid question2Id, Guid question3Id)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = questionId,
                StataExportCaption = "number1",
                Mandatory = true,
                IsInteger = true,
            }, questionnaireId, null);

            questionnaireDocument.Add(new MultyOptionsQuestion()
            {
                QuestionType = QuestionType.MultyOption,
                PublicKey = question1Id,
                StataExportCaption = "multy1",
                Mandatory = true,
            }, questionnaireId, null);

            questionnaireDocument.Add(new SingleQuestion()
            {
                QuestionType = QuestionType.SingleOption,
                PublicKey = question2Id,
                StataExportCaption = "single1",
                Mandatory = true,
            }, questionnaireId, null);

            questionnaireDocument.Add(new DateTimeQuestion()
            {
                QuestionType = QuestionType.SingleOption,
                PublicKey = question3Id,
                StataExportCaption = "date1",
                Mandatory = true,
            }, questionnaireId, null);

            return questionnaireDocument;
        }

        public static QuestionnaireDocument CreateQuestionnairDocumenteHavingNestedRosterWithConditions(Guid questionnaireId,
            Guid questionId, Guid roster1Id,
            Guid question2Id, Guid roster2Id)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = questionId,
                StataExportCaption = "q1",
                IsInteger = true,
            }, questionnaireId, null);


            questionnaireDocument.Add(new Group()
            {
                PublicKey = roster1Id,
                IsRoster = true,
                RosterSizeQuestionId = questionId,
                VariableName = "R1"
            }, questionnaireId, null);


            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = question2Id,
                StataExportCaption = "q2",
                IsInteger = true,
            }, roster1Id, null);

            questionnaireDocument.Add(new Group()
            {
                PublicKey = roster2Id,
                IsRoster = true,
                RosterSizeQuestionId = questionId,
                ConditionExpression = "q2 > 5",
                VariableName = "R1_1"
            }, roster1Id, null);

            return questionnaireDocument;
        }

        public static QuestionnaireDocument CreateQuestionnairDocumenteHavingTwoRostersInOneScopeWithConditions(Guid questionnaireId,
            Guid questionId,
            Guid roster1Id, Guid question2Id, Guid roster2Id)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };

            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = questionId,
                StataExportCaption = "q1",
                IsInteger = true,
            }, questionnaireId, null);


            questionnaireDocument.Add(new Group()
            {
                PublicKey = roster1Id,
                IsRoster = true,
                RosterSizeQuestionId = questionId,
                VariableName = "R1"
            }, questionnaireId, null);


            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = question2Id,
                StataExportCaption = "q2",
                IsInteger = true,
            }, roster1Id, null);

            questionnaireDocument.Add(new Group()
            {
                PublicKey = roster2Id,
                IsRoster = true,
                RosterSizeQuestionId = questionId,
                ConditionExpression = "q2 > 5",
                VariableName = "R2"
            }, questionnaireId, null);

            return questionnaireDocument;
        }

        public static IInterviewExpressionState GetInterviewExpressionState(QuestionnaireDocument questionnaireDocument)
        {
            var expressionProcessorGenerator = CreateExpressionProcessorGenerator();

            string resultAssembly;
            var emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(
                questionnaireDocument, new Version(6, 0, 0), out resultAssembly);

            var filePath = Path.GetTempFileName();

            if (emitResult.Success && !string.IsNullOrEmpty(resultAssembly))
            {
                File.WriteAllBytes(filePath, Convert.FromBase64String(resultAssembly));

                var compiledAssembly = Assembly.LoadFrom(filePath);

                Type interviewExpressionStateType =
                    compiledAssembly.GetTypes()
                        .FirstOrDefault(type => type.GetInterfaces().Contains(typeof (IInterviewExpressionState)));

                if (interviewExpressionStateType == null)
                    throw new Exception("Type InterviewExpressionState was not found");

                var interviewExpressionState = Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionState;

                if (interviewExpressionState == null)
                    throw new Exception("Error on IInterviewExpressionState generation");

                return interviewExpressionState;
            }

            throw new Exception("Error on IInterviewExpressionState generation");
        }

        public static IExpressionProcessorGenerator CreateExpressionProcessorGenerator(ICodeGenerator codeGenerator = null, IDynamicCompiler dynamicCompiler = null)
        {
            return
                new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(new FileSystemIOAccessor()),
                    new CodeGenerator(),
                    new DefaultDynamicCompilerSettingsProvider()
                    {
                        DynamicCompilerSettings = new DefaultDynamicCompilerSettings()
                        {
                            PortableAssembliesPath =
                                "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.5\\Profile\\Profile111",
                            DefaultReferencedPortableAssemblies = new[] { "System.dll", "System.Core.dll", "mscorlib.dll", "System.Runtime.dll", 
                                    "System.Collections.dll", "System.Linq.dll" }
                        }
                    });
        }

        public static QuestionnaireDocument CreateQuestionnaireWithQuestionAndConditionContainingUsageOfSelf(Guid questionId)
        {
            return new QuestionnaireDocument()
            {
                Children = new List<IComposite>()
                {
                    new Group("Chapter")
                    {
                        Children = new List<IComposite>()
                        {
                            new NumericQuestion("Numeric")
                            {
                                QuestionType = QuestionType.Numeric,
                                PublicKey = questionId,
                                ConditionExpression = "self > 0"
                            }
                        }
                    }
                }
            };
        }

        public static QuestionnaireDocument CreateQuestionnaireDocumenteWithOneNumericIntegerQuestionWithValidationUsingSelf(Guid questionnaireId,
            Guid questionId)
        {
            QuestionnaireDocument questionnaireDocument = new QuestionnaireDocument() { PublicKey = questionnaireId };
            Guid chapterId = Guid.Parse("23232323232323232323232323232323");
            
            questionnaireDocument.AddChapter(chapterId);
            questionnaireDocument.Add(new NumericQuestion()
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = questionId,
                StataExportCaption = "test",
                IsInteger = true,
                ValidationExpression = "self >= 1"
            }, chapterId, null);

            return questionnaireDocument;
        }
    }
}
