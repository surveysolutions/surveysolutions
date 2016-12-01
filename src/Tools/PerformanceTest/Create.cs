using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Core.SharedKernels.QuestionnaireEntities;
using CodeGenerator = WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.CodeGenerator;

namespace PerformanceTest
{
    internal static class Create
    {
        public static CodeGenerator CodeGenerator(
            IMacrosSubstitutionService macrosSubstitutionService = null,
            IExpressionProcessor expressionProcessor = null,
            ILookupTableService lookupTableService = null)
        {
            return new CodeGenerator(
                macrosSubstitutionService ?? DefaultMacrosSubstitutionService(),
                expressionProcessor ?? ServiceLocator.Current.GetInstance<IExpressionProcessor>(),
                lookupTableService ?? ServiceLocator.Current.GetInstance<ILookupTableService>(),
                Create.FileSystemIOAccessor(),
                GetCompilerSettingsStub());
        }

        private static ICompilerSettings GetCompilerSettingsStub()
            => System.Environment.MachineName.ToLower() == "powerglide" // TLK's :)
                ? Mock.Of<ICompilerSettings>(settings
                    => settings.EnableDump == false
                    && settings.DumpFolder == "C:/Projects/Data/Tests/CodeDump")
                : Mock.Of<ICompilerSettings>();

        public static IMacrosSubstitutionService DefaultMacrosSubstitutionService()
        {
            var macrosSubstitutionServiceMock = new Mock<IMacrosSubstitutionService>();
            macrosSubstitutionServiceMock.Setup(
                x => x.InlineMacros(It.IsAny<string>(), It.IsAny<IEnumerable<Macro>>()))
                .Returns((string e, IEnumerable<Macro> macros) =>
                {
                    return e;
                });

            return macrosSubstitutionServiceMock.Object;
        }

        public static Group NumericRoster(Guid? rosterId, string variable, Guid? rosterSizeQuestionId, params IComposite[] children)
        {
            Group group = Create.Group(
                id: rosterId,
                title: "Roster X",
                variable: variable,
                children: children);

            group.IsRoster = true;
            group.RosterSizeSource = RosterSizeSourceType.Question;
            group.RosterSizeQuestionId = rosterSizeQuestionId;
            return group;
        }

        public static Group MultiRoster(Guid? rosterId, string variable, Guid? sizeQuestionId, string enablementCondition = null, params IComposite[] children)
        {
            Group group = Create.Group(
                id: rosterId,
                title: "Roster X",
                variable: variable,
                enablementCondition: enablementCondition,
                children: children);

            group.IsRoster = true;
            group.RosterSizeSource = RosterSizeSourceType.Question;
            group.RosterSizeQuestionId = sizeQuestionId;
            return group;
        }

        public static SingleQuestion SingleOptionQuestion(Guid? questionId = null, string variable = null, string enablementCondition = null, string validationExpression = null,
            Guid? linkedToQuestionId = null, Guid? cascadeFromQuestionId = null, decimal[] answerCodes = null, string title = null, bool hideIfDisabled = false, string linkedFilterExpression = null,
            Guid? linkedToRosterId = null)
        {
            return new SingleQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = variable ?? "single_option_question",
                QuestionText = title ?? "SO Question",
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                QuestionType = QuestionType.SingleOption,
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                CascadeFromQuestionId = cascadeFromQuestionId,
                Answers = (answerCodes ?? new decimal[] { 1, 2, 3 }).Select(a => Create.Answer(a.ToString(), a)).ToList(),
                LinkedFilterExpression = linkedFilterExpression
            };
        }

        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Guid? id = null, params IComposite[] children)
            => Create.QuestionnaireDocument(id: id, children: Create.Chapter(children: children));

        public static QuestionnaireDocument QuestionnaireDocument(Guid? id = null, params IComposite[] children)
            => new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = (children?.ToList() ?? new List<IComposite>()).ToReadOnlyCollection(),
            };

        public static Group Chapter(string title = "Chapter X", IEnumerable<IComposite> children = null)
            => Create.Group(title: title, children: children);

        public static IQuestion Question(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null)
        {
            return new TextQuestion("Question X")
            {
                PublicKey = id ?? Guid.NewGuid(),
                QuestionType = QuestionType.Text,
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
            };
        }

        public static MultyOptionsQuestion MultiQuestion(Guid? id = null,
            IEnumerable<Answer> options = null, Guid? linkedToQuestionId = null, string variable = null,
            Guid? linkedToRosterId = null,
            bool yesNo = false,
            string optionsFilter = null,
            int maxAllowedAnswers = 60)
        {
            var multyOptionsQuestion = new MultyOptionsQuestion
            {
                QuestionType = QuestionType.MultyOption,
                PublicKey = id ?? Guid.NewGuid(),
                Answers = linkedToQuestionId.HasValue ? null : new List<Answer>(options ?? new Answer[] { }),
                LinkedToQuestionId = linkedToQuestionId,
                StataExportCaption = variable,
                LinkedToRosterId = linkedToRosterId,
                YesNoView = yesNo,
                Properties = { OptionsFilterExpression = optionsFilter },
                MaxAllowedAnswers = maxAllowedAnswers
            };
            return multyOptionsQuestion;
        }

        public static TextListQuestion ListQuestion(Guid? id = null, string variable = null, string enablementCondition = null,
            string validationExpression = null)
        {
            return new TextListQuestion
            {
                QuestionType = QuestionType.TextList,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
            };
        }

        public static TextQuestion TextQuestion(Guid? id = null, string variable = null, string enablementCondition = null,
            string validationExpression = null)
        {
            return new TextQuestion
            {
                QuestionType = QuestionType.Text,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
            };
        }

        public static NumericQuestion NumericIntegerQuestion(Guid? id = null,
            string variable = null,
            string enablementCondition = null,
            string validationExpression = null,
            IEnumerable<ValidationCondition> validationConditions = null)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = true,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationConditions = validationConditions?.ToList(),
            };
        }

        public static NumericQuestion NumericIntegerQuestion(Guid id, string variable, IList<ValidationCondition> validationExpression)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id,
                StataExportCaption = variable,
                IsInteger = true,
                ValidationConditions = validationExpression ?? new List<ValidationCondition>()
            };
        }

        public static SingleQuestion SingleQuestion(Guid? id = null, string variable = null, string enablementCondition = null,
            string validationExpression = null, Guid? cascadeFromQuestionId = null, List<Answer> options = null, Guid? linkedToQuestionId = null,
            Guid? linkedToRosterId = null, string optionsFilter = null, string linkedFilter = null)
        {
            var singleQuestion = new SingleQuestion
            {
                QuestionType = QuestionType.SingleOption,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                Answers = options ?? new List<Answer>(),
                CascadeFromQuestionId = cascadeFromQuestionId,
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                LinkedFilterExpression = linkedFilter,
                Properties =
                {
                    OptionsFilterExpression = optionsFilter
                }
            };
            return singleQuestion;
        }

        public static Answer Option(int value, string text = null)
        {
            return new Answer
            {
                AnswerText = text ?? ("Option " + value),
                AnswerCode = value,
            };
        }

        public static Answer Option(string value = null, Guid? id = null, string text = null, string parentValue = null)
        {
            return new Answer
            {
                PublicKey = id ?? Guid.NewGuid(),
                AnswerText = text ?? ("Option " + value),
                AnswerValue = value ?? "1",
                ParentValue = parentValue
            };
        }

        public static NumericQuestion NumericRealQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = false,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression
            };
        }

        public static DateTimeQuestion DateTimeQuestion(Guid id, string variable, string enablementCondition = null, string validationExpression = null)
        {
            return new DateTimeQuestion
            {
                PublicKey = id,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression
            };
        }

        public static GpsCoordinateQuestion GpsCoordinateQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null)
        {
            return new GpsCoordinateQuestion
            {
                QuestionType = QuestionType.GpsCoordinates,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression
            };
        }

        public static Group Roster(Guid? id = null,
            string title = "Roster X",
            string variable = null,
            string enablementCondition = null,
            string[] fixedTitles = null,
            IEnumerable<IComposite> children = null,
            RosterSizeSourceType rosterSizeSourceType = RosterSizeSourceType.FixedTitles,
            Guid? rosterSizeQuestionId = null,
            Guid? rosterTitleQuestionId = null,
            FixedRosterTitle[] fixedRosterTitles = null)
        {
            Group group = Create.Group(
                id: id,
                title: title,
                variable: variable,
                enablementCondition: enablementCondition,
                children: children);

            group.IsRoster = true;
            group.RosterSizeSource = rosterSizeSourceType;
            if (rosterSizeSourceType == RosterSizeSourceType.FixedTitles)
            {
                if (fixedRosterTitles == null)
                {
                    group.FixedRosterTitles =
                        (fixedTitles ?? new[] { "Roster X-1", "Roster X-2", "Roster X-3" }).Select(
                            (x, i) => Create.FixedRosterTitle(i, x)).ToArray();
                }
                else
                {
                    group.FixedRosterTitles = fixedRosterTitles;
                }
            }
            group.RosterSizeQuestionId = rosterSizeQuestionId;
            group.RosterTitleQuestionId = rosterTitleQuestionId;

            return group;
        }

        public static Group Group(
            Guid? id = null, string title = "Group X", string variable = null,
            string enablementCondition = null, IEnumerable<IComposite> children = null)
            => new Group(title)
            {
                PublicKey = id ?? Guid.NewGuid(),
                VariableName = variable,
                ConditionExpression = enablementCondition,
                Children = (children != null ? children.ToList() : new List<IComposite>()).ToReadOnlyCollection(),
            };

        public static StaticText StaticText(
            Guid? id = null, string enablementCondition = null, IEnumerable<ValidationCondition> validationConditions = null)
            => new StaticText(
                id ?? Guid.NewGuid(),
                "Static Text",
                enablementCondition,
                false,
                validationConditions?.ToList());


        public static ISubstitutionService SubstitutionService()
          => new SubstitutionService();

        public static ISubstitionTextFactory SubstitionTextFactory()
        {
            return new SubstitionTextFactory(Create.SubstitutionService(), Create.VariableToUIStringService());
        }

        private static IVariableToUIStringService VariableToUIStringService()
        {
            return new VariableToUIStringService();
        }

        public static Interview Interview(Guid? questionnaireId = null,
            IQuestionnaireStorage questionnaireRepository = null, IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null)
        {
            var interview = new Interview(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                expressionProcessorStatePrototypeProvider ?? Mock.Of<IInterviewExpressionStatePrototypeProvider>(),
                Create.SubstitionTextFactory());

            interview.CreateInterview(
                questionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"),
                1,
                new Guid("D222D222D222D222D222D222D222D222"),
                new Dictionary<Guid, AbstractAnswer>(),
                new DateTime(2012, 12, 20),
                new Guid("F111F111F111F111F111F111F111F111"));

            return interview;
        }


        public static FileSystemIOAccessor FileSystemIOAccessor()
        {
            return new FileSystemIOAccessor();
        }

        public static Answer Answer(string answer, decimal value, decimal? parentValue = null)
        {
            return new Answer()
            {
                AnswerText = answer,
                AnswerValue = value.ToString(),
                ParentValue = parentValue.HasValue ? parentValue.ToString() : null
            };
        }

        public static FixedRosterTitle FixedRosterTitle(decimal value, string title = null)
        {
            return new FixedRosterTitle(value, title ?? ("Roster " + value));
        }

        public static DesignerEngineVersionService DesignerEngineVersionService()
            => new DesignerEngineVersionService();

    }
}