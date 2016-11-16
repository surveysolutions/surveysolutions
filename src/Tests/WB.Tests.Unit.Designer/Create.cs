extern alias designer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V5.Templates;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Designer.Code;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Models;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;
using Questionnaire = WB.Core.BoundedContexts.Designer.Aggregates.Questionnaire;
using QuestionnaireVersion = WB.Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireVersion;
using QuestionnaireView = WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireView;
using Translation = WB.Core.SharedKernels.SurveySolutions.Documents.Translation;
using TranslationInstance = WB.Core.BoundedContexts.Designer.Translations.TranslationInstance;

namespace WB.Tests.Unit.Designer
{
    internal static partial class Create
    {
        public static User AccountDocument(string userName = "", Guid? userId = null)
            => new User
            {
                ProviderUserKey = userId ?? Guid.NewGuid(),
                UserName = userName,
            };

        public static Answer Answer(string answer = "answer option", decimal? value = null, string stringValue = null, decimal? parentValue = null)
        {
            return new Answer()
            {
                AnswerText = answer,
                AnswerValue = stringValue ?? value.ToString(),
                ParentValue = parentValue.HasValue ? parentValue.ToString() : null
            };
        }

        public static Attachment Attachment(Guid? attachmentId = null, string name = "attachment", string contentId = "content id")
        {
            return new Attachment
            {
                AttachmentId = attachmentId ?? Guid.NewGuid(),
                Name = name,
                ContentId = contentId
            };
        }

        public static AttachmentContent AttachmentContent(byte[] content = null, string contentType = null, string contentId = null,  long? size = null, AttachmentDetails details = null)
        {
            return new AttachmentContent
            {
                ContentId = contentId,
                Content = content ?? new byte[0],
                ContentType = contentType ?? "whatever",
                Size = size ?? 10,
                Details = details
            };
        }

        public static AttachmentMeta AttachmentMeta(
            Guid attachmentId,
            string contentHash,
            Guid questionnaireId,
            string fileName = null,
            DateTime? lastUpdateDate = null)
        {
            return new AttachmentMeta
            {
                AttachmentId = attachmentId,
                ContentId = contentHash,
                QuestionnaireId = questionnaireId,
                FileName = fileName ?? "fileName.txt",
                LastUpdateDate = lastUpdateDate ?? DateTime.UtcNow
            };
        }

        public static AttachmentSize AttachmentSize(long? size = null)
        {
            return new AttachmentSize
            {
                Size = size ?? 10
            };
        }

        public static AttachmentView AttachmentView(Guid? id = null, long? size = null)
        {
            return new AttachmentView
            {
                AttachmentId = (id ?? Guid.NewGuid()).FormatGuid(),
                Meta = new AttachmentMeta {},
                Content = new AttachmentContent
                {
                    Size = size ?? 10
                }
            };
        }

        public static Group Chapter(string title = "Chapter X", Guid? chapterId = null, bool hideIfDisabled = false, IEnumerable<IComposite> children = null)
        {
            return Create.Group(
                title: title,
                groupId: chapterId,
                hideIfDisabled: hideIfDisabled,
                children: children);
        }

        public static Group Section(string title = "Section X", Guid? sectionId = null, IEnumerable<IComposite> children = null)
            => Create.Group(
                title: title,
                groupId: sectionId,
                children: children);

        public static Group Subsection(string title = "Subsection X", Guid? sectionId = null, IEnumerable<IComposite> children = null)
            => Create.Group(
                title: title,
                groupId: sectionId,
                children: children);

        public static CodeGenerationSettings CodeGenerationSettings()
        {
            return new CodeGenerationSettings(additionInterfaces: new[] { "IInterviewExpressionStateV5" },
                namespaces: new[]
                {
                    "WB.Core.SharedKernels.DataCollection.V2",
                    "WB.Core.SharedKernels.DataCollection.V2.CustomFunctions",
                    "WB.Core.SharedKernels.DataCollection.V3.CustomFunctions",
                    "WB.Core.SharedKernels.DataCollection.V4",
                    "WB.Core.SharedKernels.DataCollection.V4.CustomFunctions",
                    "WB.Core.SharedKernels.DataCollection.V5",
                    "WB.Core.SharedKernels.DataCollection.V5.CustomFunctions"
                },
                isLookupTablesFeatureSupported: true,
                expressionStateBodyGenerator: expressionStateModel => new InterviewExpressionStateTemplateV5(expressionStateModel).TransformText());
        }

        public static CodeGenerator CodeGenerator(
            IMacrosSubstitutionService macrosSubstitutionService = null,
            IExpressionProcessor expressionProcessor = null,
            ILookupTableService lookupTableService = null)
        {
            return new CodeGenerator(
                macrosSubstitutionService ?? Create.DefaultMacrosSubstitutionService(),
                expressionProcessor ?? ServiceLocator.Current.GetInstance<IExpressionProcessor>(),
                lookupTableService ?? ServiceLocator.Current.GetInstance<ILookupTableService>(),
                Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ICompilerSettings>());
        }

        public static QuestionProperties QuestionProperties()
        {
            return new QuestionProperties(false, false);
        }

        public static DateTimeQuestion DateTimeQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string variable = null, string validationMessage = null, string text = null, QuestionScope scope = QuestionScope.Interviewer,
            bool preFilled = false, bool hideIfDisabled = false, bool isCurrentTime = false)
        {
            return new DateTimeQuestion("Question DT")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                QuestionText = text,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = variable,
                QuestionScope = scope,
                Featured = preFilled,
                IsTimestamp = isCurrentTime,
            };
        }

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

        public static IDesignerEngineVersionService DesignerEngineVersionService()
        {
            return new DesignerEngineVersionService();
        }

        public static DownloadQuestionnaireRequest DownloadQuestionnaireRequest(Guid? questionnaireId, QuestionnaireVersion questionnaireVersion = null)
        {
            return new DownloadQuestionnaireRequest()
            {
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                SupportedVersion = questionnaireVersion ?? new QuestionnaireVersion()
            };
        }

        public static FixedRosterTitle FixedRosterTitle(decimal value, string title)
        {
            return new FixedRosterTitle(value, title);
        }

        public static GenerationResult GenerationResult(bool success = false)
        {
            return new GenerationResult() { Success = success };
        }


        private static Guid GetQuestionnaireItemId(string questionnaireItemId)
        {
            return string.IsNullOrEmpty(questionnaireItemId) ? Guid.NewGuid() : Guid.Parse(questionnaireItemId);
        }

        private static Guid? GetQuestionnaireItemParentId(string questionnaireItemParentId)
        {
            return string.IsNullOrEmpty(questionnaireItemParentId)
                ? (Guid?)null
                : Guid.Parse(questionnaireItemParentId);
        }

        public static GpsCoordinateQuestion GpsCoordinateQuestion(Guid? questionId = null, string variable = "var1", bool isPrefilled = false, string title = null,
            string enablementCondition = null, string validationExpression = null, bool hideIfDisabled = false)
        {
            return new GpsCoordinateQuestion()
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = variable,
                QuestionType = QuestionType.GpsCoordinates,
                Featured = isPrefilled,
                QuestionText = title,
                ValidationExpression = validationExpression,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
            };
        }

        public static Group Group(
            Guid? groupId = null,
            string title = "Group X",
            string variable = null,
            string enablementCondition = null,
            bool hideIfDisabled = false,
            IEnumerable<IComposite> children = null,
            Guid? rosterSizeQuestionId = null)
        {
            return new Group(title)
            {
                PublicKey = groupId ?? Guid.NewGuid(),
                VariableName = variable,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>()),
                RosterSizeQuestionId = rosterSizeQuestionId,
            };
        }

        public static KeywordsProvider KeywordsProvider()
        {
            return new KeywordsProvider(Create.SubstitutionService());
        }

        public static LookupTable LookupTable(string tableName, string fileName = null)
        {
            return new LookupTable
            {
                TableName = tableName,
                FileName = fileName ?? "lookup.tab"
            };
        }

        public static LookupTableContent LookupTableContent(string[] variableNames, params LookupTableRow[] rows)
        {
            return new LookupTableContent
            {
                VariableNames = variableNames,
                Rows = rows
            };
        }

        public static LookupTableRow LookupTableRow(long rowcode, decimal?[] values)
        {
            return new LookupTableRow
            {
                RowCode = rowcode,
                Variables = values
            };
        }

        public static LookupTableService LookupTableService(
            IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage = null,
            IPlainKeyValueStorage<QuestionnaireDocument> documentStorage = null)
        {
            return new LookupTableService(
                lookupTableContentStorage ?? Mock.Of<IPlainKeyValueStorage<LookupTableContent>>(),
                documentStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>());
        }

        public static Macro Macro(string name, string content = null, string description = null)
        {
            return new Macro
            {
                Name = name,
                Content = content,
                Description = description
            };
        }

        public static MacrosSubstitutionService MacrosSubstitutionService()
            => new MacrosSubstitutionService();


        public static MultimediaQuestion MultimediaQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string variable = null, string validationMessage = null, string title = null, QuestionScope scope = QuestionScope.Interviewer
            , bool hideIfDisabled = false)
        {
            return new MultimediaQuestion("Question T")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionType = QuestionType.Multimedia,
                StataExportCaption = variable,
                QuestionScope = scope,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                QuestionText = title
            };
        }

        public static IMultyOptionsQuestion MultipleOptionsQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            bool areAnswersOrdered = false, int? maxAllowedAnswers = null, Guid? linkedToQuestionId = null, bool isYesNo = false, bool hideIfDisabled = false, List<Answer> answersList = null,
            string title = null,
            params decimal[] answers)
        {
            return new MultyOptionsQuestion("Question MO")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                StataExportCaption = "mo_question",
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                AreAnswersOrdered = areAnswersOrdered,
                MaxAllowedAnswers = maxAllowedAnswers,
                QuestionType = QuestionType.MultyOption,
                LinkedToQuestionId = linkedToQuestionId,
                YesNoView = isYesNo,
                Answers = answersList ?? answers.Select(a => Create.Answer(a.ToString(), a)).ToList(),
                QuestionText = title,
            };
        }

        public static MultyOptionsQuestion MultyOptionsQuestion(Guid? id = null,
            IEnumerable<Answer> options = null, Guid? linkedToQuestionId = null, string variable = null, bool yesNoView = false,
            string enablementCondition = null, string validationExpression = null, Guid? linkedToRosterId = null,
            int? maxAllowedAnswers = null, string title = null)
        {
            return new MultyOptionsQuestion
            {
                QuestionType = QuestionType.MultyOption,
                PublicKey = id ?? Guid.NewGuid(),
                Answers = linkedToQuestionId.HasValue ? null : new List<Answer>(options ?? new Answer[] { }),
                LinkedToQuestionId = linkedToQuestionId,
                LinkedToRosterId = linkedToRosterId,
                StataExportCaption = variable,
                YesNoView = yesNoView,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                MaxAllowedAnswers = maxAllowedAnswers
            };
        }

        public static NumericQuestion NumericIntegerQuestion(Guid? id = null, string variable = "numeric_question", string enablementCondition = null,
            string validationExpression = null, QuestionScope scope = QuestionScope.Interviewer, bool isPrefilled = false,
            bool hideIfDisabled = false, IEnumerable<ValidationCondition> validationConditions = null, Guid? linkedToRosterId = null,
            string title = null)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = true,
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                QuestionScope = scope,
                Featured = isPrefilled,
                ValidationConditions = validationConditions?.ToList() ?? new List<ValidationCondition>(),
                LinkedToRosterId = linkedToRosterId,
                QuestionText = title
            };
        }

        public static NumericQuestion NumericRealQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null, IEnumerable<ValidationCondition> validationConditions = null,
            string title = null)
        {
            return new NumericQuestion
            {
                QuestionType = QuestionType.Numeric,
                PublicKey = id ?? Guid.NewGuid(),
                StataExportCaption = variable,
                IsInteger = false,
                ConditionExpression = enablementCondition,
                ValidationConditions = validationConditions?.ToList() ?? new List<ValidationCondition>(),
                ValidationExpression = validationExpression,
                QuestionText = title
            };
        }


        public static Answer Option(int code, string text = null, string parentValue = null, Guid? id = null)
        {
            return new Answer
            {
                PublicKey = id ?? Guid.NewGuid(),
                AnswerText = text ?? "text",
                ParentValue = parentValue,
                AnswerCode = code
            };
        }

        public static Answer Option(string value = null, string text = null, string parentValue = null, Guid? id = null)
        {
            return new Answer
            {
                PublicKey = id ?? Guid.NewGuid(),
                AnswerText = text ?? "text",
                AnswerValue = value ?? "1",
                ParentValue = parentValue
            };
        }

        public static QRBarcodeQuestion QRBarcodeQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string variable = null, string validationMessage = null, string text = null, QuestionScope scope = QuestionScope.Interviewer, bool preFilled = false,
            bool hideIfDisabled = false)
        {
            return new QRBarcodeQuestion()
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                QuestionText = text,
                QuestionType = QuestionType.QRBarcode,
                StataExportCaption = variable,
                QuestionScope = scope,
                Featured = preFilled
            };
        }

        public static IQuestion Question(
            Guid? questionId = null,
            string variable = "question",
            string enablementCondition = null,
            string validationExpression = null,
            string validationMessage = null,
            QuestionType questionType = QuestionType.Text,
            IEnumerable<ValidationCondition> validationConditions = null,
            string variableLabel =null,
            string title= "Question X test",
            string instructions = null,
            bool isPrefilled = false,
            params Answer[] answers)
        {
            return new TextQuestion(title)
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                QuestionType = questionType,
                StataExportCaption = variable,
                ConditionExpression = enablementCondition,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                VariableLabel = variableLabel,
                Instructions = instructions,
                Answers = answers.ToList(),
                ValidationConditions = validationConditions?.ToList() ?? new List<ValidationCondition>(),
                Featured = isPrefilled,
            };
        }

        public static Questionnaire Questionnaire(IExpressionProcessor expressionProcessor = null, IQuestionnireHistotyVersionsService histotyVersionsService = null)
        {
            return new Questionnaire(
                Mock.Of<ILogger>(),
                Mock.Of<IClock>(),
                expressionProcessor ?? Mock.Of<IExpressionProcessor>(),
                Create.SubstitutionService(),
                Create.KeywordsProvider(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<IAttachmentService>(),
                Mock.Of<ITranslationsService>(),
                histotyVersionsService ?? Mock.Of<IQuestionnireHistotyVersionsService>());
        }


        public static Questionnaire Questionnaire(Guid responsible, QuestionnaireDocument document)
        {
            var questionnaire = new Questionnaire(
                Mock.Of<ILogger>(),
                Mock.Of<IClock>(),
                Mock.Of<IExpressionProcessor>(),
                Create.SubstitutionService(),
                Create.KeywordsProvider(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<IAttachmentService>(),
                Mock.Of<ITranslationsService>(),
                Mock.Of<IQuestionnireHistotyVersionsService>());
            questionnaire.Initialize(Guid.NewGuid(), document, new List<SharedPerson> {Create.SharedPerson(responsible)});
            return questionnaire;
        }


        public static QuestionnaireChangeRecord QuestionnaireChangeRecord(
            string questionnaireId = null,
            QuestionnaireActionType? action = null,
            Guid? targetId = null,
            QuestionnaireItemType? targetType = null,
            params QuestionnaireChangeReference[] reference)
        {
            return new QuestionnaireChangeRecord()
            {
                QuestionnaireId = questionnaireId,
                ActionType = action ?? QuestionnaireActionType.Add,
                TargetItemId = targetId ?? Guid.NewGuid(),
                TargetItemType = targetType ?? QuestionnaireItemType.Group,
                References = reference.ToHashSet()
            };
        }

        public static QuestionnaireChangeReference QuestionnaireChangeReference(
            Guid? referenceId = null,
            QuestionnaireItemType? referenceType = null)
        {
            return new QuestionnaireChangeReference()
            {
                ReferenceId = referenceId ?? Guid.NewGuid(),
                ReferenceType = referenceType ?? QuestionnaireItemType.Group
            };
        }

        public static QuestionnaireDocument QuestionnaireDocument(Guid? id = null, params IComposite[] children)
            => Create.QuestionnaireDocument(id: id, children: children, title: "Questionnaire X");

        public static Variable Variable(Guid? id = null, VariableType type = VariableType.LongInteger, string variableName = "v1", string expression = "2*2")
        {
            return new Variable(publicKey: id ?? Guid.NewGuid(),
                variableData: new VariableData(type: type, name: variableName, expression: expression, label: null));
        }

        public static QuestionnaireDocument QuestionnaireDocument(
            Guid? id = null, string title = null, IEnumerable<IComposite> children = null, Guid? userId = null)
        {
            return new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>()),
                Title = title,
                CreatedBy = userId ?? Guid.NewGuid()
            };
        }

        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(IEnumerable<IComposite> children)
            => QuestionnaireDocumentWithOneChapter(null, null, null, null, children.ToArray());

        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(params IComposite[] children)
            => QuestionnaireDocumentWithOneChapter(null, null, null, null, children);

        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Guid? chapterId = null, params IComposite[] children)
        {
            return QuestionnaireDocumentWithOneChapter(null, chapterId, null, null, children);
        }

        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Attachment[] attachments = null, params IComposite[] children)
        {
            return QuestionnaireDocumentWithOneChapter(null, null, attachments, null, children);
        }

        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Translation[] translations = null, params IComposite[] children)
        {
            return QuestionnaireDocumentWithOneChapter(null, null, null, translations, children);
        }
        
        public static QuestionnaireDocument QuestionnaireDocumentWithOneChapter(Guid? questionnaireId = null, Guid? chapterId = null, Attachment[] attachments = null, 
            Translation[] translations = null, params IComposite[] children)
        {
            var result = new QuestionnaireDocument
            {
                PublicKey = questionnaireId ?? Guid.NewGuid(),
                Children = new IComposite[]
                {
                    new Group("Chapter")
                    {
                        PublicKey = chapterId.GetValueOrDefault(),
                        Children = children.ToReadOnlyCollection()
                    }
                }.ToReadOnlyCollection()
            };

            result.Attachments.AddRange(attachments ?? new Attachment[0]);
            result.Translations.AddRange(translations ?? new Translation[0]);

            return result;
        }

        public static QuestionnaireExpressionStateModelFactory QuestionnaireExecutorTemplateModelFactory(
            IMacrosSubstitutionService macrosSubstitutionService = null,
            IExpressionProcessor expressionProcessor = null,
            ILookupTableService lookupTableService = null)
        {
            return new QuestionnaireExpressionStateModelFactory(
                macrosSubstitutionService ?? Create.DefaultMacrosSubstitutionService(),
                expressionProcessor ?? ServiceLocator.Current.GetInstance<IExpressionProcessor>(),
                lookupTableService ?? ServiceLocator.Current.GetInstance<ILookupTableService>());
        }

        public static QuestionnaireSharedPersons QuestionnaireSharedPersons(Guid? questionnaireId = null)
        {
            return new QuestionnaireSharedPersons(questionnaireId ?? Guid.NewGuid());
        }

        public static QuestionnaireStateTracker QuestionnaireStateTacker()
        {
            return new QuestionnaireStateTracker();
        }

        public static QuestionnaireView QuestionnaireView(Guid? createdBy)
            => Create.QuestionnaireView(new QuestionnaireDocument { CreatedBy = createdBy ?? Guid.NewGuid() });

        public static QuestionnaireView QuestionnaireView(QuestionnaireDocument questionnaireDocument = null, IEnumerable<SharedPerson> sharedPersons = null)
            => new QuestionnaireView(questionnaireDocument ?? Create.QuestionnaireDocument(), sharedPersons ?? Enumerable.Empty<SharedPerson>());

        public static RoslynExpressionProcessor RoslynExpressionProcessor() => new RoslynExpressionProcessor();

        public static Group FixedRoster(Guid? rosterId = null, IEnumerable<string> fixedTitles = null, IEnumerable<IComposite> children = null, 
            string variable = "roster_var", string title = "Roster X", FixedRosterTitle[] fixedRosterTitles = null)
            => Create.Roster(
                rosterId: rosterId,
                children: children,
                variable: variable,
                title: title,
                fixedTitles: fixedTitles?.ToArray() ?? new[] { "Fixed Roster 1", "Fixed Roster 2", "Fixed Roster 3" },
                fixedRosterTitles: fixedRosterTitles);

        public static Group Roster(
            Guid? rosterId = null,
            string title = "Roster X",
            string variable = "roster_var",
            string enablementCondition = null,
            string[] fixedTitles = null,
            IEnumerable<IComposite> children = null,
            RosterSizeSourceType rosterType = RosterSizeSourceType.FixedTitles,
            Guid? rosterSizeQuestionId = null,
            Guid? rosterTitleQuestionId = null,
            FixedRosterTitle[] fixedRosterTitles = null)
        {
            Group group = Create.Group(
                groupId: rosterId,
                title: title,
                variable: variable,
                enablementCondition: enablementCondition,
                children: children);

            group.IsRoster = true;
            group.RosterSizeSource = rosterType;

            if (rosterType == RosterSizeSourceType.FixedTitles)
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


        public static SingleQuestion SingleOptionQuestion(Guid? questionId = null, string variable = null, string enablementCondition = null, string validationExpression = null,
            Guid? linkedToQuestionId = null, Guid? cascadeFromQuestionId = null, decimal[] answerCodes = null, string title = null, bool hideIfDisabled = false, string linkedFilterExpression = null,
            Guid? linkedToRosterId = null, List<Answer> answers = null, bool isPrefilled = false, bool isComboBox = false)
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
                Answers = answers ?? (answerCodes ?? new decimal[] { 1, 2, 3 }).Select(a => Create.Answer(a.ToString(), a)).ToList(),
                LinkedFilterExpression = linkedFilterExpression,
                Featured = isPrefilled,
                IsFilteredCombobox = isComboBox,
            };
        }

        public static SingleQuestion SingleQuestion(Guid? id = null, string variable = null, string enablementCondition = null, string validationExpression = null,
            Guid? cascadeFromQuestionId = null, List<Answer> options = null, Guid? linkedToQuestionId = null, QuestionScope scope = QuestionScope.Interviewer,
            bool isFilteredCombobox = false, Guid? linkedToRosterId = null, string optionsFilter = null, bool isPrefilled = false,
            string linkedFilter = null, string title = null)
        {
            return new SingleQuestion
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
                QuestionScope = scope,
                IsFilteredCombobox = isFilteredCombobox,
                Featured = isPrefilled,
                Properties = {OptionsFilterExpression = optionsFilter},
                QuestionText = title,
            };
        }


        public static StaticText StaticText(
            Guid? staticTextId = null,
            string text = "Static Text X",
            string attachmentName = null,
            string enablementCondition = null,
            bool hideIfDisabled = false,
            IEnumerable<ValidationCondition> validationConditions = null)
        {
            return new StaticText(
                staticTextId ?? Guid.NewGuid(), 
                text,
                enablementCondition,
                hideIfDisabled,
                validationConditions?.ToList() ?? new List<ValidationCondition>(),
                attachmentName);
        }

        public static ISubstitutionService SubstitutionService()
        {
            return new SubstitutionService();
        }

        public static ITextListQuestion TextListQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            int? maxAnswerCount = null, string variable = null, bool hideIfDisabled = false, string title = null)
        {
            return new TextListQuestion("Question TL")
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                MaxAnswerCount = maxAnswerCount,
                QuestionType = QuestionType.TextList,
                StataExportCaption = variable,
                QuestionText = title,
            };
        }

        public static TextQuestion TextQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string mask = null,
            string variable = "text_question",
            string validationMessage = null,
            string text = "Question T",
            QuestionScope scope = QuestionScope.Interviewer,
            bool preFilled = false,
            string label = null,
            string instruction = null,
            IEnumerable<ValidationCondition> validationConditions = null,
            bool hideIfDisabled = false)

        {
            return new TextQuestion(text)
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Mask = mask,
                QuestionText = text,
                QuestionType = QuestionType.Text,
                StataExportCaption = variable,
                QuestionScope = scope,
                Featured = preFilled,
                VariableLabel = label,
                Instructions = instruction,
                ValidationConditions = validationConditions?.ToList().ConcatWithOldConditionIfNotEmpty(validationExpression, validationMessage),
                
            };
        }

        public static QuestionnaireVerificationMessage VerificationError(string code, string message, IEnumerable<string> compilationErrorMessages, params QuestionnaireNodeReference[] questionnaireNodeReferences)
        {
            return QuestionnaireVerificationMessage.Error(code, message, compilationErrorMessages, questionnaireNodeReferences);
        }

        public static QuestionnaireVerificationMessage VerificationError(string code, string message, params QuestionnaireNodeReference[] questionnaireNodeReferences)
        {
            return QuestionnaireVerificationMessage.Error(code, message, questionnaireNodeReferences);
        }

        public static QuestionnaireVerificationMessage VerificationWarning(string code, string message, params QuestionnaireNodeReference[] questionnaireNodeReferences)
        {
            return QuestionnaireVerificationMessage.Warning(code, message, questionnaireNodeReferences);
        }

        public static VerificationMessage VerificationMessage(string code, string message, params VerificationReferenceEnriched[] references)
        {
            return new VerificationMessage
            {
                Code = code,
                Message = message,
                Errors = new List<VerificationMessageError>()
                {
                    new VerificationMessageError()
                    {
                        References = references.ToList()
                    }
                }
            };
        }

        public static QuestionnaireNodeReference VerificationReference(Guid? id = null, QuestionnaireVerificationReferenceType type = QuestionnaireVerificationReferenceType.Question)
        {
            return new QuestionnaireNodeReference(type, id ?? Guid.NewGuid());
        }

        public static VerificationReferenceEnriched VerificationReferenceEnriched(QuestionnaireVerificationReferenceType type, Guid id, string title)
        {
            return new VerificationReferenceEnriched
            {
                Type = type,
                ItemId = id.FormatGuid(),
                Title = title
            };
        }

        internal static class Command
        {
            public static AddLookupTable AddLookupTable(Guid questionnaireId, Guid lookupTableId, Guid responsibleId, string lookupTableName = "table")
            {
                return new AddLookupTable(questionnaireId, lookupTableName, null, lookupTableId, responsibleId);
            }

            public static AddMacro AddMacro(Guid questionnaire, Guid? macroId = null, Guid? userId = null)
            {
                return new AddMacro(questionnaire, macroId ?? Guid.NewGuid(), userId ?? Guid.NewGuid());
            }

            public static DeleteLookupTable DeleteLookupTable(Guid questionnaireId, Guid lookupTableId, Guid responsibleId)
            {
                return new DeleteLookupTable(questionnaireId, lookupTableId, responsibleId);
            }

            public static DeleteMacro DeleteMacro(Guid questionnaire, Guid? macroId = null, Guid? userId = null)
            {
                return new DeleteMacro(questionnaire, macroId ?? Guid.NewGuid(), userId ?? Guid.NewGuid());
            }

            public static UpdateLookupTable UpdateLookupTable(Guid questionnaireId, Guid lookupTableId, Guid responsibleId, 
                string lookupTableName = "table", Guid? oldLookupTableId = null)
            {
                return new UpdateLookupTable(questionnaireId, lookupTableId, responsibleId, lookupTableName, "file", oldLookupTableId);
            }

            internal static UpdateMacro UpdateMacro(Guid questionnaireId, Guid macroId, string name, string content, string description, Guid? userId)
            {
                return new UpdateMacro(questionnaireId, macroId, name, content, description, userId ?? Guid.NewGuid());
            }

            public static UpdateStaticText UpdateStaticText(Guid questionnaireId, Guid entityId, string text, string attachmentName, Guid responsibleId,
                string enablementCondition, bool hideIfDisabled = false, IList<ValidationCondition> validationConditions = null)
            {
                return new UpdateStaticText(questionnaireId, entityId, text, attachmentName, responsibleId, enablementCondition, hideIfDisabled, validationConditions);
            }
            
            public static AddOrUpdateAttachment AddOrUpdateAttachment(Guid questionnaireId, Guid attachmentId, string attachmentContentId, 
                Guid responsibleId, string attachmentName, Guid? oldAttachmentId = null)
            {
                return new AddOrUpdateAttachment(questionnaireId, attachmentId, responsibleId, attachmentName, attachmentContentId, oldAttachmentId);
            }

            public static DeleteAttachment DeleteAttachment(Guid questionnaireId, Guid attachmentId, Guid responsibleId)
            {
                return new DeleteAttachment(questionnaireId, attachmentId, responsibleId);
            }

            public static UpdateGroup UpdateGroup(Guid questionnaireId, Guid groupId, Guid? responsibleId = null,
                string title = null, string variableName = null, Guid? rosterSizeQuestionId = null,
                string condition = null, bool hideIfDisabled = false, bool isRoster = false,
                RosterSizeSourceType rosterSizeSource = RosterSizeSourceType.Question,
                FixedRosterTitleItem[] fixedRosterTitles = null, Guid? rosterTitleQuestionId = null)
                => new UpdateGroup(questionnaireId, groupId, responsibleId ?? Guid.NewGuid(), title, variableName,
                    rosterSizeQuestionId, condition, hideIfDisabled, isRoster,
                    rosterSizeSource, fixedRosterTitles, rosterTitleQuestionId);

            public static UpdateVariable UpdateVariable(Guid questionnaireId, Guid entityId, VariableType type, string name, string expression, Guid? userId = null)
            {
                return new UpdateVariable(questionnaireId, userId ?? Guid.NewGuid(), entityId, new VariableData(type, name, expression, null));
            }

            public static AddOrUpdateTranslation AddOrUpdateTranslation(Guid questionnaireId, Guid translationId, string name, 
                Guid responsibleId, Guid? oldTranslationId = null)
            {
                return new AddOrUpdateTranslation(questionnaireId, responsibleId, translationId, name, oldTranslationId);
            }

            public static DeleteTranslation DeleteTranslation(Guid questionnaireId, Guid translationId, Guid responsibleId)
            {
                return new DeleteTranslation(questionnaireId, responsibleId, translationId);
            }

            public static MoveGroup MoveGroup(Guid questionnaireId, Guid groupId, Guid responsibleId, Guid? targetGroupId = null, int? tagretIndex = null)
                => new MoveGroup(questionnaireId, groupId, targetGroupId, tagretIndex ?? 0, responsibleId);

            public static MoveStaticText MoveStaticText(Guid questionnaireId, Guid staticTextId, Guid responsibleId,
                Guid? targetGroupId = null, int? tagretIndex = null)
                => new MoveStaticText(questionnaireId, staticTextId, targetGroupId ?? Guid.NewGuid(), tagretIndex ?? 0,
                    responsibleId);

            public static MoveVariable MoveVariable(Guid questionnaireId, Guid variableId, Guid responsibleId,
                Guid? targetGroupId = null, int? tagretIndex = null)
                => new MoveVariable(questionnaireId, variableId, targetGroupId ?? Guid.NewGuid(), tagretIndex ?? 0,
                    responsibleId);

            public static MoveQuestion MoveQuestion(Guid questionnaireId, Guid questionId, Guid responsibleId,
                Guid? targetGroupId = null, int? targetIndex = null)
                => new MoveQuestion(questionnaireId, questionId, targetGroupId ?? Guid.NewGuid(), targetIndex ?? 0,
                    responsibleId);

            public static ImportQuestionnaire ImportQuestionnaire(QuestionnaireDocument questionnaireDocument, Guid? responsibleId = null)
                => new ImportQuestionnaire(responsibleId ?? Guid.NewGuid(), questionnaireDocument);

            public static PasteAfter PasteAfter(Guid questionnaireId, Guid entityId, Guid itemToPasteAfterId, 
                Guid sourceQuestionnaireId, Guid sourceItemId, Guid responsibleId) 
                => new PasteAfter(questionnaireId, entityId, itemToPasteAfterId, sourceQuestionnaireId, sourceItemId, responsibleId);

            public static DeleteGroup DeleteGroup(Guid questionnaireId, Guid groupId)
                => new DeleteGroup(questionnaireId, groupId, Guid.NewGuid());

            public static ReplaceTextsCommand ReplaceTextsCommand(string searchFor, 
                string replaceWith, 
                bool matchWholeWord = false, 
                bool matchCase = false, 
                bool useRegex = false,
                Guid? userId = null)
            {
                return new ReplaceTextsCommand(Guid.Empty, userId ?? Guid.Empty, searchFor.ToLower(), replaceWith, matchCase, matchWholeWord, useRegex);
            }
            
            public static AddStaticText AddStaticText(Guid questionnaireId, Guid staticTextId, string text, Guid responsibleId, Guid parentId)
            {
                return new AddStaticText(questionnaireId, staticTextId, text, responsibleId, parentId);
            }
            
            public static AddDefaultTypeQuestion AddDefaultTypeQuestion(Guid questionnaireId, Guid questionId, string text, Guid responsibleId, Guid parentId, int? tagretIndex = null)
            {
                return new AddDefaultTypeQuestion(questionnaireId, questionId, parentId, text, responsibleId, tagretIndex);
            }

            public static UpdateNumericQuestion UpdateNumericQuestion(Guid questionnaireId, Guid questionId, Guid responsibleId, 
                string title, bool isPreFilled = false, QuestionScope scope = QuestionScope.Interviewer, bool isInteger = false, 
                bool useFormatting = false, int? countOfDecimalPlaces = null, List<ValidationCondition> validationConditions = null)
            {
                return new UpdateNumericQuestion(questionnaireId, questionId, responsibleId, new CommonQuestionParameters {Title = title}, isPreFilled, scope, 
                    isInteger, useFormatting, countOfDecimalPlaces, validationConditions ?? new List<ValidationCondition>());
            }

            public static AddVariable AddVariable(Guid questionnaireId, Guid entityId, Guid parentId, Guid responsibleId, string name = null, string expression = null, VariableType variableType = VariableType.String, int? index =null)
            {
                return new AddVariable(questionnaireId, entityId, new VariableData(variableType, name, expression, null), responsibleId, parentId, index);
            }

            public static UpdateQuestionnaire UpdateQuestionnaire(Guid questionnaireId, Guid responsibleId, string title = "title", bool isPublic = false, bool isResponsibleAdmin = false)
            {
                return new UpdateQuestionnaire(questionnaireId, title, isPublic, responsibleId, isResponsibleAdmin);
            }
            public static DeleteQuestionnaire DeleteQuestionnaire(Guid questionnaireId, Guid responsibleId)
            {
                return new DeleteQuestionnaire(questionnaireId, responsibleId);
            }

            public static RevertVersionQuestionnaire RevertVersionQuestionnaire(Guid questionnaireId, Guid historyReferanceId, Guid responsibleId)
            {
                return new RevertVersionQuestionnaire(questionnaireId, historyReferanceId, responsibleId);
            }
        }

        public static ValidationCondition ValidationCondition(string expression = "self != null", string message = "should be answered")
        {
            return new ValidationCondition(expression, message);
        }

        public static AttachmentService AttachmentService(
            IPlainStorageAccessor<AttachmentContent> attachmentContentStorage = null,
            IPlainStorageAccessor<AttachmentMeta> attachmentMetaStorage = null)
        {
            return new AttachmentService(attachmentContentStorage: attachmentContentStorage,
                attachmentMetaStorage: attachmentMetaStorage);
        }

        public static ITopologicalSorter<T> TopologicalSorter<T>()
        {
            return new TopologicalSorter<T>();
        }

        public static SharedPerson SharedPerson(Guid? id = null, string email = null, bool isOwner = true, ShareType shareType = ShareType.Edit)
        {
            return new SharedPerson
            {
                Id = id ?? Guid.NewGuid(),
                IsOwner = isOwner,
                Email = email ?? "user@e.mail",
                ShareType = shareType
            };
        }

        public static TranslationInstance TranslationInstance(Guid? questionnaireId = null,
            TranslationType type = TranslationType.Unknown,
            Guid? questionnaireEntityId = null,
            string translationIndex = null,
            Guid? translationId = null,
            string translation = null)
        {
            return new TranslationInstance
            {
                QuestionnaireId = questionnaireId ?? Guid.NewGuid(),
                QuestionnaireEntityId = questionnaireEntityId ?? Guid.NewGuid(),
                Type = type,
                Value = translation,
                TranslationIndex = translationIndex,
                TranslationId = translationId ?? Guid.NewGuid()
            };
        }

        public static TranslationDto TranslationDto(
            TranslationType type = TranslationType.Unknown,
            Guid? questionnaireEntityId = null,
            string translationIndex = null,
            Guid? translationId = null,
            string translation = null)
        {
            return new TranslationDto
            {
                QuestionnaireEntityId = questionnaireEntityId ?? Guid.NewGuid(),
                Type = type,
                Value = translation,
                TranslationIndex = translationIndex,
                TranslationId = translationId ?? Guid.NewGuid()
            };
        }

        public static QuestionnaireTranslator QuestionnaireTranslator()
            => new QuestionnaireTranslator();

        public static Translation Translation(Guid? translationId = null, string name = null)
        {
            return new Translation() { Name = name, Id = translationId ?? Guid.NewGuid() };
        }

        public static TranslationsService TranslationsService(
            IPlainStorageAccessor<TranslationInstance> traslationsStorage = null,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage = null)
            => new TranslationsService(
                traslationsStorage ?? new TestPlainStorage<TranslationInstance>(),
                questionnaireStorage ?? Stub<IPlainKeyValueStorage<QuestionnaireDocument>>.Returning(Create.QuestionnaireDocument()));


        public static DeskAuthenticationService DeskAuthenticationService(string multipassKey, string returnUrlFormat, string siteKey)
        {
            return new DeskAuthenticationService(new DeskSettings(multipassKey, returnUrlFormat, siteKey));
        }

        public static UpdateQuestionnaire UpdateQuestionnaire(string title, bool isPublic, Guid responsibleId, bool isResponsibleAdmin = false)
            => new UpdateQuestionnaire(Guid.NewGuid(), title, isPublic, responsibleId, isResponsibleAdmin);

        public static QuestionnaireListViewItem QuestionnaireListViewItem(bool isPublic = false)
        {
            return new QuestionnaireListViewItem() { IsPublic = isPublic };
        }

        public static HistoryPostProcessor HistoryPostProcessor() => new HistoryPostProcessor();

        public static CustomWebApiAuthorizeFilter CustomWebApiAuthorizeFilter()
        {
            return new CustomWebApiAuthorizeFilter();
        }

        public static QuestionnaireVerifier QuestionnaireVerifier(
            IExpressionProcessor expressionProcessor = null,
            ISubstitutionService substitutionService = null,
            IKeywordsProvider keywordsProvider = null,
            IExpressionProcessorGenerator expressionProcessorGenerator = null,
            IMacrosSubstitutionService macrosSubstitutionService = null,
            ILookupTableService lookupTableService = null,
            IAttachmentService attachmentService = null,
            ITopologicalSorter<string> topologicalSorter = null,
            IQuestionnaireTranslator questionnaireTranslator = null)
        {
            var fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.MakeStataCompatibleFileName(Moq.It.IsAny<string>())).Returns<string>(s => s);

            var questionnireExpressionProcessorGeneratorMock = new Mock<IExpressionProcessorGenerator>();
            string generationResult;
            questionnireExpressionProcessorGeneratorMock.Setup(
                _ => _.GenerateProcessorStateAssembly(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<int>(), out generationResult))
                .Returns(new GenerationResult() { Success = true, Diagnostics = new List<GenerationDiagnostic>() });

            var substitutionServiceInstance = new SubstitutionService();

            var lookupTableServiceMock = new Mock<ILookupTableService>(MockBehavior.Default)
            {
                DefaultValue = DefaultValue.Mock
            };

            var attachmentServiceMock = Stub<IAttachmentService>.WithNotEmptyValues;

            return new QuestionnaireVerifier(expressionProcessor ?? new Mock<IExpressionProcessor>().Object,
                fileSystemAccessorMock.Object,
                substitutionService ?? substitutionServiceInstance,
                keywordsProvider ?? new KeywordsProvider(substitutionServiceInstance),
                expressionProcessorGenerator ?? questionnireExpressionProcessorGeneratorMock.Object,
                new DesignerEngineVersionService(),
                macrosSubstitutionService ?? Create.DefaultMacrosSubstitutionService(),
                lookupTableService ?? lookupTableServiceMock.Object,
                attachmentService ?? attachmentServiceMock,
                topologicalSorter ?? Create.TopologicalSorter<string>(),
                Mock.Of<ITranslationsService>(),
                questionnaireTranslator ?? Mock.Of<IQuestionnaireTranslator>());
        }
    }
}