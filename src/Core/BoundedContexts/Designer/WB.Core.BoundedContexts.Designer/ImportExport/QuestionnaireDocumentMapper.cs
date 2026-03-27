using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;
using WB.Core.BoundedContexts.Designer.ImportExport.Models.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;
using AbstractQuestion = Main.Core.Entities.SubEntities.AbstractQuestion;
using Answer = Main.Core.Entities.SubEntities.Answer;
using AreaQuestion = Main.Core.Entities.SubEntities.Question.AreaQuestion;
using AudioQuestion = Main.Core.Entities.SubEntities.Question.AudioQuestion;
using DateTimeQuestion = Main.Core.Entities.SubEntities.Question.DateTimeQuestion;
using Documents = WB.Core.SharedKernels.SurveySolutions.Documents;
using GeometryType = WB.Core.SharedKernels.Questionnaire.Documents.GeometryType;
using GeometryInputMode = WB.Core.SharedKernels.Questionnaire.Documents.GeometryInputMode;
using GpsCoordinateQuestion = Main.Core.Entities.SubEntities.Question.GpsCoordinateQuestion;
using IQuestion = Main.Core.Entities.SubEntities.IQuestion;
using NumericQuestion = Main.Core.Entities.SubEntities.Question.NumericQuestion;
using QRBarcodeQuestion = Main.Core.Entities.SubEntities.Question.QRBarcodeQuestion;
using Group = Main.Core.Entities.SubEntities.Group;
using QuestionnaireEntities = WB.Core.SharedKernels.QuestionnaireEntities;
using SingleQuestion = Main.Core.Entities.SubEntities.Question.SingleQuestion;
using StaticText = Main.Core.Entities.SubEntities.StaticText;
using TextListQuestion = Main.Core.Entities.SubEntities.Question.TextListQuestion;
using TextQuestion = Main.Core.Entities.SubEntities.Question.TextQuestion;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public class QuestionnaireDocumentMapper
    {
        public Questionnaire Map(QuestionnaireDocument doc)
        {
            var idToVarMap = GetIdToVariableNameMap(doc);
            return MapDocumentToQuestionnaire(doc, idToVarMap);
        }

        public QuestionnaireDocument Map(Questionnaire questionnaire)
        {
            var varToIdMap = GenerateVariableNameToIdMap(questionnaire);
            var doc = MapQuestionnaireToDocument(questionnaire, varToIdMap);
            FixAfterMapping(doc);
            return doc;
        }

        private Dictionary<Guid, string> GetIdToVariableNameMap(QuestionnaireDocument doc)
        {
            var map = new Dictionary<Guid, string>();
            doc.ForEachTreeElement<IComposite>(c => c.Children, (parent, child) =>
            {
                map[child.PublicKey] = child.VariableName;
            });
            return map;
        }

        private Dictionary<string, Guid> GenerateVariableNameToIdMap(Questionnaire questionnaire)
        {
            var map = new Dictionary<string, Guid>();

            var coverPageEntities = questionnaire.CoverPage?.TreeToEnumerable<Models.QuestionnaireEntity>(x =>
            {
                if (x is Models.CoverPage coverPage)
                    return coverPage.Children;
                return Enumerable.Empty<Models.QuestionnaireEntity>();
            }) ?? Enumerable.Empty<Models.QuestionnaireEntity>();

            var questionnaireEntities = questionnaire.Children.TreeToEnumerable<Models.QuestionnaireEntity>(x =>
            {
                if (x is Models.Group group)
                    return group.Children;
                return Enumerable.Empty<Models.QuestionnaireEntity>();
            });

            foreach (var entity in coverPageEntities.Concat(questionnaireEntities))
            {
                if (entity is Models.Question.AbstractQuestion question && !question.VariableName.IsNullOrEmpty())
                    map[question.VariableName!] = question.Id ?? Guid.NewGuid();
                else if (entity is Models.Group grp && !grp.VariableName.IsNullOrEmpty())
                    map[grp.VariableName!] = grp.Id ?? Guid.NewGuid();
                else if (entity is Models.Variable variable && !variable.VariableName.IsNullOrEmpty())
                    map[variable.VariableName!] = variable.Id ?? Guid.NewGuid();
            }

            return map;
        }

        private static void FixAfterMapping(QuestionnaireDocument doc)
        {
            doc.ForEachTreeElement<IComposite>(c => c.Children, (parent, child) =>
            {
                if (child is IQuestion question)
                {
                    if (question.LinkedToQuestionId.HasValue &&
                        question.LinkedToQuestionId == question.LinkedToRosterId)
                    {
                        var linkedToQuestion = doc.Find<IQuestion>(question.LinkedToQuestionId.Value);
                        question.LinkedToQuestionId = linkedToQuestion?.PublicKey;
                        var linkedToRoster = doc.Find<IGroup>(question.LinkedToRosterId.Value);
                        question.LinkedToRosterId = linkedToRoster?.PublicKey;
                    }
                }
            });
        }

        // ===================== Document → Model (Export) =====================

        private Questionnaire MapDocumentToQuestionnaire(QuestionnaireDocument doc, Dictionary<Guid, string> idToVarMap)
        {
            var questionnaire = new Questionnaire
            {
                Id = doc.PublicKey,
                Title = doc.Title,
                Description = doc.Description,
                VariableName = doc.VariableName,
                HideIfDisabled = doc.HideIfDisabled,
                Metadata = MapMetaInfo(doc.Metadata),
                Macros = doc.Macros.Values.Select(MapMacro).ToList(),
                LookupTables = doc.LookupTables.Values.Select(MapLookupTable).ToList(),
                Attachments = doc.Attachments.Select(MapAttachment).ToList(),
                Categories = doc.Categories.Select(MapCategories).ToList(),
                CriticalRules = doc.CriticalRules.Select(MapCriticalRule).ToList(),
                Translations = new Models.Translations
                {
                    DefaultTranslation = doc.DefaultTranslation,
                    OriginalDisplayName = doc.DefaultLanguageName,
                    Items = doc.Translations.Select(MapTranslation).ToList()
                }
            };

            // Handle cover page
            if (doc.Children.Count > 0 && doc.Children[0].PublicKey == doc.CoverPageSectionId)
            {
                var coverGroup = (Group)doc.Children[0];
                questionnaire.CoverPage = MapGroupToCoverPage(coverGroup, idToVarMap);
                questionnaire.Children = doc.Children.Skip(1)
                    .Select(c => MapCompositeToGroup(c, idToVarMap))
                    .Where(c => c != null)
                    .Select(c => c!)
                    .ToList();
            }
            else
            {
                // Cover page not found as first child - gather identified items
                var identifiedItems = ((IComposite)doc)
                    .TreeToEnumerableDepthFirst(c => c.Children)
                    .Where(IsIdentified)
                    .ToList();
                var coverGroup = new Group("Cover", identifiedItems)
                {
                    PublicKey = doc.CoverPageSectionId,
                };
                questionnaire.CoverPage = MapGroupToCoverPage(coverGroup, idToVarMap);
                questionnaire.Children = doc.Children
                    .Select(c => MapCompositeToGroup(c, idToVarMap))
                    .Where(c => c != null)
                    .Select(c => c!)
                    .ToList();
            }

            return questionnaire;
        }

        private Models.CoverPage MapGroupToCoverPage(Group group, Dictionary<Guid, string> idToVarMap)
        {
            return new Models.CoverPage
            {
                Id = group.PublicKey,
                Title = group.Title,
                VariableName = group.VariableName,
                Children = group.Children.Select(c => MapCompositeToEntity(c, idToVarMap))
                    .Where(c => c != null)
                    .Select(c => c!)
                    .ToList()
            };
        }

        private Models.Group? MapCompositeToGroup(IComposite composite, Dictionary<Guid, string> idToVarMap)
        {
            if (composite is Group group)
            {
                if (group.IsRoster)
                    return MapGroupToRoster(group, idToVarMap);
                return MapGroupToGroup(group, idToVarMap);
            }
            return null;
        }

        private Models.QuestionnaireEntity? MapCompositeToEntity(IComposite composite, Dictionary<Guid, string> idToVarMap)
        {
            return composite switch
            {
                Group group when group.IsRoster => MapGroupToRoster(group, idToVarMap),
                Group group => MapGroupToGroup(group, idToVarMap),
                StaticText staticText => MapStaticText(staticText, idToVarMap),
                QuestionnaireEntities.Variable variable => MapVariable(variable, idToVarMap),
                TextQuestion q => MapTextQuestion(q, idToVarMap),
                NumericQuestion q => MapNumericQuestion(q, idToVarMap),
                AreaQuestion q => MapAreaQuestion(q, idToVarMap),
                AudioQuestion q => MapAudioQuestion(q, idToVarMap),
                DateTimeQuestion q => MapDateTimeQuestion(q, idToVarMap),
                GpsCoordinateQuestion q => MapGpsQuestion(q, idToVarMap),
                MultimediaQuestion q => MapPictureQuestion(q, idToVarMap),
                MultyOptionsQuestion q => MapMultiOptionsQuestion(q, idToVarMap),
                QRBarcodeQuestion q => MapQRBarcodeQuestion(q, idToVarMap),
                SingleQuestion q => MapSingleQuestion(q, idToVarMap),
                TextListQuestion q => MapTextListQuestion(q, idToVarMap),
                _ => null
            };
        }

        private Models.Group MapGroupToGroup(Group group, Dictionary<Guid, string> idToVarMap)
        {
            return new Models.Group
            {
                Id = group.PublicKey,
                Title = group.Title,
                VariableName = group.VariableName,
                ConditionExpression = group.ConditionExpression,
                HideIfDisabled = group.HideIfDisabled,
                Children = group.Children
                    .Where(e => !IsIdentified(e))
                    .Select(c => MapCompositeToEntity(c, idToVarMap))
                    .Where(c => c != null)
                    .Select(c => c!)
                    .ToList()
            };
        }

        private Models.Roster MapGroupToRoster(Group group, Dictionary<Guid, string> idToVarMap)
        {
            bool hasCustomTitle = group.CustomRosterTitle
                || group.DisplayMode == Main.Core.Entities.SubEntities.RosterDisplayMode.Matrix
                || group.DisplayMode == Main.Core.Entities.SubEntities.RosterDisplayMode.Table;
            string rosterTitle = hasCustomTitle ? group.Title : group.Title + @" - %rostertitle%";

            return new Models.Roster
            {
                Id = group.PublicKey,
                Title = rosterTitle,
                VariableName = group.VariableName,
                ConditionExpression = group.ConditionExpression,
                HideIfDisabled = group.HideIfDisabled,
                DisplayMode = (Models.RosterDisplayMode)(int)group.DisplayMode,
                RosterSizeSource = (Models.RosterSizeSourceType)(int)group.RosterSizeSource,
                FixedRosterTitles = group.FixedRosterTitles
                    .Select(f => new Models.FixedRosterTitle { Value = (int?)f.Value, Title = f.Title })
                    .ToArray(),
                RosterSizeQuestion = GetVarName(group.RosterSizeQuestionId, idToVarMap),
                RosterTitleQuestion = GetVarName(group.RosterTitleQuestionId, idToVarMap),
                Children = group.Children
                    .Where(e => !IsIdentified(e))
                    .Select(c => MapCompositeToEntity(c, idToVarMap))
                    .Where(c => c != null)
                    .Select(c => c!)
                    .ToList()
            };
        }

        private Models.StaticText MapStaticText(StaticText staticText, Dictionary<Guid, string> idToVarMap)
        {
            return new Models.StaticText
            {
                Id = staticText.PublicKey,
                Text = staticText.Text,
                AttachmentName = staticText.AttachmentName,
                ConditionExpression = staticText.ConditionExpression,
                HideIfDisabled = staticText.HideIfDisabled,
                ValidationConditions = staticText.ValidationConditions
                    .Select(MapValidationCondition)
                    .ToList()
            };
        }

        private Models.Variable MapVariable(QuestionnaireEntities.Variable variable, Dictionary<Guid, string> idToVarMap)
        {
            return new Models.Variable
            {
                Id = variable.PublicKey,
                VariableName = variable.Name,
                Label = variable.Label,
                Expression = variable.Expression,
                DoNotExport = variable.DoNotExport,
                VariableType = (Models.VariableType)(int)variable.Type
            };
        }

        private void MapAbstractQuestionToModel(AbstractQuestion src, Models.Question.AbstractQuestion dst, Dictionary<Guid, string> idToVarMap)
        {
            dst.Id = src.PublicKey;
            dst.VariableName = src.StataExportCaption;
            dst.Comments = src.Comments;
            dst.ConditionExpression = src.ConditionExpression;
            dst.HideIfDisabled = src.HideIfDisabled;
            dst.Instructions = src.Instructions;
            dst.HideInstructions = src.Properties?.HideInstructions ?? false;
            dst.IsCritical = src.Properties?.IsCritical ?? false;
            dst.QuestionScope = (Models.QuestionScope)(int)src.QuestionScope;
            dst.QuestionText = src.QuestionText;
            dst.VariableLabel = src.VariableLabel;
            dst.ValidationConditions = src.ValidationConditions
                .Select(MapValidationCondition)
                .ToList();
        }

        private Models.Question.TextQuestion MapTextQuestion(TextQuestion q, Dictionary<Guid, string> idToVarMap)
        {
            var dst = new Models.Question.TextQuestion { Mask = q.Mask };
            MapAbstractQuestionToModel(q, dst, idToVarMap);
            return dst;
        }

        private Models.Question.NumericQuestion MapNumericQuestion(NumericQuestion q, Dictionary<Guid, string> idToVarMap)
        {
            var dst = new Models.Question.NumericQuestion
            {
                IsInteger = q.IsInteger,
                DecimalPlaces = q.CountOfDecimalPlaces,
                UseThousandsSeparator = q.Properties?.UseFormatting ?? false,
                SpecialValues = q.Answers.Select(MapAnswerToSpecialValue).ToList()
            };
            MapAbstractQuestionToModel(q, dst, idToVarMap);
            return dst;
        }

        private Models.Question.AreaQuestion MapAreaQuestion(AreaQuestion q, Dictionary<Guid, string> idToVarMap)
        {
            var dst = new Models.Question.AreaQuestion
            {
                GeometryType = q.Properties != null && q.Properties.GeometryType.HasValue 
                    ? (Models.Question.GeometryType?)(int)q.Properties.GeometryType.Value 
                    : null,
                GeometryInputMode = q.Properties != null && q.Properties.GeometryInputMode.HasValue 
                    ? (Models.Question.GeometryInputMode?)(int)q.Properties.GeometryInputMode.Value 
                    : null,
                GeometryOverlapDetection = q.Properties?.GeometryOverlapDetection
            };
            MapAbstractQuestionToModel(q, dst, idToVarMap);
            return dst;
        }

        private Models.Question.AudioQuestion MapAudioQuestion(AudioQuestion q, Dictionary<Guid, string> idToVarMap)
        {
            var dst = new Models.Question.AudioQuestion();
            MapAbstractQuestionToModel(q, dst, idToVarMap);
            return dst;
        }

        private Models.Question.DateTimeQuestion MapDateTimeQuestion(DateTimeQuestion q, Dictionary<Guid, string> idToVarMap)
        {
            var dst = new Models.Question.DateTimeQuestion
            {
                DefaultDate = q.Properties?.DefaultDate,
                IsTimestamp = q.IsTimestamp
            };
            MapAbstractQuestionToModel(q, dst, idToVarMap);
            return dst;
        }

        private Models.Question.GpsCoordinateQuestion MapGpsQuestion(GpsCoordinateQuestion q, Dictionary<Guid, string> idToVarMap)
        {
            var dst = new Models.Question.GpsCoordinateQuestion();
            MapAbstractQuestionToModel(q, dst, idToVarMap);
            return dst;
        }

        private Models.Question.PictureQuestion MapPictureQuestion(MultimediaQuestion q, Dictionary<Guid, string> idToVarMap)
        {
            var dst = new Models.Question.PictureQuestion { IsSignature = q.IsSignature };
            MapAbstractQuestionToModel(q, dst, idToVarMap);
            return dst;
        }

        private Models.Question.QRBarcodeQuestion MapQRBarcodeQuestion(QRBarcodeQuestion q, Dictionary<Guid, string> idToVarMap)
        {
            var dst = new Models.Question.QRBarcodeQuestion();
            MapAbstractQuestionToModel(q, dst, idToVarMap);
            return dst;
        }

        private Models.Question.SingleQuestion MapSingleQuestion(SingleQuestion q, Dictionary<Guid, string> idToVarMap)
        {
            var displayMode = q.CascadeFromQuestionId.HasValue
                ? Models.Question.SingleOptionDisplayMode.Cascading
                : (q.IsFilteredCombobox == true ? Models.Question.SingleOptionDisplayMode.Combobox : Models.Question.SingleOptionDisplayMode.Radio);

            var linkedTo = GetVarName(q.LinkedToQuestionId ?? q.LinkedToRosterId, idToVarMap);
            string? filterExpression = (q.LinkedToQuestionId.HasValue || q.LinkedToRosterId.HasValue)
                ? q.LinkedFilterExpression
                : q.Properties?.OptionsFilterExpression;

            var dst = new Models.Question.SingleQuestion
            {
                ShowAsList = q.ShowAsList,
                ShowAsListThreshold = q.ShowAsListThreshold,
                CategoriesId = q.CategoriesId,
                Answers = q.Answers.Select(MapAnswerToAnswer).ToList(),
                DisplayMode = displayMode,
                CascadeFromQuestion = GetVarName(q.CascadeFromQuestionId, idToVarMap),
                LinkedTo = linkedTo,
                FilterExpression = filterExpression
            };
            MapAbstractQuestionToModel(q, dst, idToVarMap);
            return dst;
        }

        private Models.Question.MultiOptionsQuestion MapMultiOptionsQuestion(MultyOptionsQuestion q, Dictionary<Guid, string> idToVarMap)
        {
            var displayMode = q.YesNoView
                ? Models.Question.MultiOptionsDisplayMode.YesNo
                : (q.IsFilteredCombobox == true ? Models.Question.MultiOptionsDisplayMode.Combobox : Models.Question.MultiOptionsDisplayMode.Checkboxes);

            var linkedTo = GetVarName(q.LinkedToQuestionId ?? q.LinkedToRosterId, idToVarMap);
            string? filterExpression = (q.LinkedToQuestionId.HasValue || q.LinkedToRosterId.HasValue)
                ? q.LinkedFilterExpression
                : q.Properties?.OptionsFilterExpression;

            var dst = new Models.Question.MultiOptionsQuestion
            {
                AreAnswersOrdered = q.AreAnswersOrdered,
                MaxAllowedAnswers = q.MaxAllowedAnswers,
                CategoriesId = q.CategoriesId,
                Answers = q.Answers.Select(MapAnswerToAnswer).ToList(),
                DisplayMode = displayMode,
                LinkedTo = linkedTo,
                FilterExpression = filterExpression
            };
            MapAbstractQuestionToModel(q, dst, idToVarMap);
            return dst;
        }

        private Models.Question.TextListQuestion MapTextListQuestion(TextListQuestion q, Dictionary<Guid, string> idToVarMap)
        {
            var dst = new Models.Question.TextListQuestion { MaxItemsCount = q.MaxAnswerCount };
            MapAbstractQuestionToModel(q, dst, idToVarMap);
            return dst;
        }

        // ===================== Model → Document (Import) =====================

        private QuestionnaireDocument MapQuestionnaireToDocument(Questionnaire q, Dictionary<string, Guid> varToIdMap)
        {
            var doc = new QuestionnaireDocument();
            doc.PublicKey = q.Id;
            // Id is derived from PublicKey via FormatGuid() - no need to set it separately
            doc.Title = q.Title ?? string.Empty;
            doc.Description = q.Description;
            doc.VariableName = q.VariableName;
            doc.HideIfDisabled = q.HideIfDisabled;
            doc.Metadata = MapMetaInfoFromModel(q.Metadata);

            doc.Macros = q.Macros
                .ToDictionary(_ => Guid.NewGuid(), MapMacroFromModel);
            doc.LookupTables = q.LookupTables
                .ToDictionary(_ => Guid.NewGuid(), MapLookupTableFromModel);
            doc.Attachments = q.Attachments.Select(MapAttachmentFromModel).ToList();
            doc.Translations = q.Translations.Items.Select(MapTranslationFromModel).ToList();
            doc.Categories = q.Categories.Select(MapCategoriesFromModel).ToList();
            doc.CriticalRules = q.CriticalRules.Select(MapCriticalRuleFromModel).ToList();

            if (q.Translations.DefaultTranslation.HasValue)
            {
                doc.DefaultTranslation = doc.Translations
                    .FirstOrDefault(t => t.Id == q.Translations.DefaultTranslation)?.Id;
            }
            doc.DefaultLanguageName = q.Translations.OriginalDisplayName;

            // Build children list: cover page first, then other children
            var coverGroup = MapCoverPageToGroup(q.CoverPage ?? new Models.CoverPage { Id = Guid.NewGuid(), Title = "Cover" }, varToIdMap);
            var otherChildren = q.Children
                .Select(c => MapModelGroupToComposite(c, varToIdMap))
                .Where(c => c != null)
                .Select(c => c!)
                .ToList<IComposite>();

            var allChildren = new List<IComposite> { coverGroup };
            allChildren.AddRange(otherChildren);
            doc.Children = allChildren.ToReadOnlyCollection();

            doc.CoverPageSectionId = q.CoverPage != null
                ? doc.Children[0].PublicKey
                : Guid.NewGuid();

            // Set Featured=true for questions in cover page
            if (q.CoverPage != null)
            {
                doc.Children[0].Children.OfType<IQuestion>().ForEach(question => question.Featured = true);
            }

            return doc;
        }

        private Group MapCoverPageToGroup(Models.CoverPage coverPage, Dictionary<string, Guid> varToIdMap)
        {
            var publicKey = GetIdOrGenerate(coverPage.VariableName, coverPage.Id, varToIdMap);
            var children = coverPage.Children
                .Select(c => MapModelEntityToComposite(c, varToIdMap))
                .Where(c => c != null)
                .Select(c => c!)
                .ToList();

            var group = new Group(coverPage.Title, children)
            {
                PublicKey = publicKey,
                VariableName = coverPage.VariableName!,
                ConditionExpression = null!
            };
            return group;
        }

        private IComposite? MapModelGroupToComposite(Models.Group modelGroup, Dictionary<string, Guid> varToIdMap)
        {
            if (modelGroup is Models.Roster roster)
                return MapRosterToGroup(roster, varToIdMap);
            return MapGroupToSourceGroup(modelGroup, varToIdMap);
        }

        private IComposite? MapModelEntityToComposite(Models.QuestionnaireEntity entity, Dictionary<string, Guid> varToIdMap)
        {
            return entity switch
            {
                Models.Roster roster => MapRosterToGroup(roster, varToIdMap),
                Models.Group group => MapGroupToSourceGroup(group, varToIdMap),
                Models.StaticText staticText => MapStaticTextFromModel(staticText, varToIdMap),
                Models.Variable variable => MapVariableFromModel(variable, varToIdMap),
                Models.Question.TextQuestion q => MapTextQuestionFromModel(q, varToIdMap),
                Models.Question.NumericQuestion q => MapNumericQuestionFromModel(q, varToIdMap),
                Models.Question.AreaQuestion q => MapAreaQuestionFromModel(q, varToIdMap),
                Models.Question.AudioQuestion q => MapAudioQuestionFromModel(q, varToIdMap),
                Models.Question.DateTimeQuestion q => MapDateTimeQuestionFromModel(q, varToIdMap),
                Models.Question.GpsCoordinateQuestion q => MapGpsQuestionFromModel(q, varToIdMap),
                Models.Question.PictureQuestion q => MapPictureQuestionFromModel(q, varToIdMap),
                Models.Question.MultiOptionsQuestion q => MapMultiOptionsQuestionFromModel(q, varToIdMap),
                Models.Question.QRBarcodeQuestion q => MapQRBarcodeQuestionFromModel(q, varToIdMap),
                Models.Question.SingleQuestion q => MapSingleQuestionFromModel(q, varToIdMap),
                Models.Question.TextListQuestion q => MapTextListQuestionFromModel(q, varToIdMap),
                _ => null
            };
        }

        private Group MapGroupToSourceGroup(Models.Group modelGroup, Dictionary<string, Guid> varToIdMap)
        {
            var publicKey = GetIdOrGenerate(modelGroup.VariableName, modelGroup.Id, varToIdMap);
            var children = modelGroup.Children
                .Select(c => MapModelEntityToComposite(c, varToIdMap))
                .Where(c => c != null)
                .Select(c => c!)
                .ToList();

            var group = new Group(modelGroup.Title, children)
            {
                PublicKey = publicKey,
                VariableName = modelGroup.VariableName!,
                ConditionExpression = modelGroup.ConditionExpression!,
                HideIfDisabled = modelGroup.HideIfDisabled
            };
            return group;
        }

        private Group MapRosterToGroup(Models.Roster roster, Dictionary<string, Guid> varToIdMap)
        {
            var publicKey = GetIdOrGenerate(roster.VariableName, roster.Id, varToIdMap);
            var children = roster.Children
                .Select(c => MapModelEntityToComposite(c, varToIdMap))
                .Where(c => c != null)
                .Select(c => c!)
                .ToList();

            bool customRosterTitle = roster.DisplayMode != Models.RosterDisplayMode.Table
                && roster.DisplayMode != Models.RosterDisplayMode.Matrix;

            var group = new Group(roster.Title, children)
            {
                PublicKey = publicKey,
                VariableName = roster.VariableName!,
                ConditionExpression = roster.ConditionExpression!,
                HideIfDisabled = roster.HideIfDisabled,
                IsRoster = true,
                CustomRosterTitle = customRosterTitle,
                DisplayMode = (Main.Core.Entities.SubEntities.RosterDisplayMode)(int)roster.DisplayMode,
                RosterSizeSource = (Main.Core.Entities.SubEntities.RosterSizeSourceType)(int)roster.RosterSizeSource,
                FixedRosterTitles = (roster.FixedRosterTitles ?? Array.Empty<Models.FixedRosterTitle>())
                    .Select(f => new Documents.FixedRosterTitle(f.Value ?? 0, f.Title))
                    .ToArray(),
                RosterSizeQuestionId = GetId(roster.RosterSizeQuestion, varToIdMap),
                RosterTitleQuestionId = GetId(roster.RosterTitleQuestion, varToIdMap)
            };
            return group;
        }

        private StaticText MapStaticTextFromModel(Models.StaticText src, Dictionary<string, Guid> varToIdMap)
        {
            var publicKey = src.Id ?? Guid.NewGuid();
            var staticText = new StaticText(
                publicKey,
                src.Text,
                src.ConditionExpression ?? string.Empty,
                src.HideIfDisabled,
                src.ValidationConditions?.Select(MapValidationConditionFromModel).ToList(),
                src.AttachmentName,
                null
            );
            return staticText;
        }

        private QuestionnaireEntities.Variable MapVariableFromModel(Models.Variable src, Dictionary<string, Guid> varToIdMap)
        {
            var publicKey = GetIdOrGenerate(src.VariableName, src.Id, varToIdMap);
            var variable = new QuestionnaireEntities.Variable(publicKey, null)
            {
                Name = src.VariableName ?? string.Empty,
                Label = src.Label!,
                Expression = src.Expression ?? string.Empty,
                DoNotExport = src.DoNotExport,
                Type = (QuestionnaireEntities.VariableType)(int)src.VariableType
            };
            return variable;
        }

        private void MapAbstractQuestionFromModel(Models.Question.AbstractQuestion src, AbstractQuestion dst, Dictionary<string, Guid> varToIdMap)
        {
            dst.PublicKey = GetIdOrGenerate(src.VariableName, src.Id, varToIdMap);
            dst.StataExportCaption = src.VariableName ?? string.Empty;
            dst.Comments = src.Comments;
            dst.ConditionExpression = src.ConditionExpression!;
            dst.HideIfDisabled = src.HideIfDisabled;
            dst.Instructions = src.Instructions;
            dst.QuestionScope = (Main.Core.Entities.SubEntities.QuestionScope)(int)src.QuestionScope;
            dst.QuestionText = src.QuestionText;
            dst.VariableLabel = src.VariableLabel;
            dst.ValidationConditions = src.ValidationConditions?
                .Select(MapValidationConditionFromModel)
                .ToList<QuestionnaireEntities.ValidationCondition>()
                ?? new List<QuestionnaireEntities.ValidationCondition>();

            dst.Properties!.HideInstructions = src.HideInstructions;
            dst.Properties!.IsCritical = src.IsCritical;
        }

        private TextQuestion MapTextQuestionFromModel(Models.Question.TextQuestion src, Dictionary<string, Guid> varToIdMap)
        {
            var q = new TextQuestion { Mask = src.Mask };
            MapAbstractQuestionFromModel(src, q, varToIdMap);
            return q;
        }

        private NumericQuestion MapNumericQuestionFromModel(Models.Question.NumericQuestion src, Dictionary<string, Guid> varToIdMap)
        {
            var q = new NumericQuestion
            {
                IsInteger = src.IsInteger,
                CountOfDecimalPlaces = src.DecimalPlaces,
                UseFormatting = src.UseThousandsSeparator,
                Answers = src.SpecialValues?.Select(MapSpecialValueToAnswer).ToList() ?? new List<Answer>()
            };
            MapAbstractQuestionFromModel(src, q, varToIdMap);
            q.Properties!.UseFormatting = src.UseThousandsSeparator;
            return q;
        }

        private AreaQuestion MapAreaQuestionFromModel(Models.Question.AreaQuestion src, Dictionary<string, Guid> varToIdMap)
        {
            var q = new AreaQuestion();
            MapAbstractQuestionFromModel(src, q, varToIdMap);
            q.Properties!.GeometryType = src.GeometryType.HasValue ? (GeometryType?)(int)src.GeometryType.Value : null;
            q.Properties!.GeometryInputMode = src.GeometryInputMode.HasValue ? (GeometryInputMode?)(int)src.GeometryInputMode.Value : null;
            q.Properties!.GeometryOverlapDetection = src.GeometryOverlapDetection;
            return q;
        }

        private AudioQuestion MapAudioQuestionFromModel(Models.Question.AudioQuestion src, Dictionary<string, Guid> varToIdMap)
        {
            var q = new AudioQuestion();
            MapAbstractQuestionFromModel(src, q, varToIdMap);
            return q;
        }

        private DateTimeQuestion MapDateTimeQuestionFromModel(Models.Question.DateTimeQuestion src, Dictionary<string, Guid> varToIdMap)
        {
            var q = new DateTimeQuestion { IsTimestamp = src.IsTimestamp };
            MapAbstractQuestionFromModel(src, q, varToIdMap);
            q.Properties!.DefaultDate = src.DefaultDate;
            return q;
        }

        private GpsCoordinateQuestion MapGpsQuestionFromModel(Models.Question.GpsCoordinateQuestion src, Dictionary<string, Guid> varToIdMap)
        {
            var q = new GpsCoordinateQuestion();
            MapAbstractQuestionFromModel(src, q, varToIdMap);
            return q;
        }

        private MultimediaQuestion MapPictureQuestionFromModel(Models.Question.PictureQuestion src, Dictionary<string, Guid> varToIdMap)
        {
            var q = new MultimediaQuestion { IsSignature = src.IsSignature };
            MapAbstractQuestionFromModel(src, q, varToIdMap);
            return q;
        }

        private QRBarcodeQuestion MapQRBarcodeQuestionFromModel(Models.Question.QRBarcodeQuestion src, Dictionary<string, Guid> varToIdMap)
        {
            var q = new QRBarcodeQuestion();
            MapAbstractQuestionFromModel(src, q, varToIdMap);
            return q;
        }

        private SingleQuestion MapSingleQuestionFromModel(Models.Question.SingleQuestion src, Dictionary<string, Guid> varToIdMap)
        {
            var q = new SingleQuestion
            {
                ShowAsList = src.ShowAsList,
                ShowAsListThreshold = src.ShowAsListThreshold,
                CategoriesId = src.CategoriesId,
                Answers = src.Answers?.Select(MapAnswerFromModel).ToList() ?? new List<Answer>(),
                IsFilteredCombobox = src.DisplayMode == Models.Question.SingleOptionDisplayMode.Combobox ? true : (bool?)null,
                LinkedToQuestionId = GetId(src.LinkedTo, varToIdMap),
                LinkedToRosterId = GetId(src.LinkedTo, varToIdMap),
                CascadeFromQuestionId = GetId(src.CascadeFromQuestion, varToIdMap)
            };
            MapAbstractQuestionFromModel(src, q, varToIdMap);
            if (!src.LinkedTo.IsNullOrEmpty())
                q.LinkedFilterExpression = src.FilterExpression;
            else
                q.Properties!.OptionsFilterExpression = src.FilterExpression;
            return q;
        }

        private MultyOptionsQuestion MapMultiOptionsQuestionFromModel(Models.Question.MultiOptionsQuestion src, Dictionary<string, Guid> varToIdMap)
        {
            var q = new MultyOptionsQuestion
            {
                AreAnswersOrdered = src.AreAnswersOrdered,
                MaxAllowedAnswers = src.MaxAllowedAnswers,
                CategoriesId = src.CategoriesId,
                Answers = src.Answers?.Select(MapAnswerFromModel).ToList() ?? new List<Answer>(),
                YesNoView = src.DisplayMode == Models.Question.MultiOptionsDisplayMode.YesNo,
                IsFilteredCombobox = src.DisplayMode == Models.Question.MultiOptionsDisplayMode.Combobox ? true : (bool?)null,
                LinkedToQuestionId = GetId(src.LinkedTo, varToIdMap),
                LinkedToRosterId = GetId(src.LinkedTo, varToIdMap)
            };
            MapAbstractQuestionFromModel(src, q, varToIdMap);
            if (!src.LinkedTo.IsNullOrEmpty())
                q.LinkedFilterExpression = src.FilterExpression;
            else
                q.Properties!.OptionsFilterExpression = src.FilterExpression;
            return q;
        }

        private TextListQuestion MapTextListQuestionFromModel(Models.Question.TextListQuestion src, Dictionary<string, Guid> varToIdMap)
        {
            var q = new TextListQuestion { MaxAnswerCount = src.MaxItemsCount };
            MapAbstractQuestionFromModel(src, q, varToIdMap);
            return q;
        }

        // ===================== Simple type mappings =====================

        private static Models.QuestionnaireMetaInfo? MapMetaInfo(WB.Core.SharedKernels.Questionnaire.Documents.QuestionnaireMetaInfo? src)
        {
            if (src == null) return null;
            return new Models.QuestionnaireMetaInfo
            {
                SubTitle = src.SubTitle,
                StudyType = src.StudyType,
                Version = src.Version,
                VersionNotes = src.VersionNotes,
                KindOfData = src.KindOfData,
                Country = src.Country,
                Year = src.Year,
                Language = src.Language,
                Coverage = src.Coverage,
                Universe = src.Universe,
                UnitOfAnalysis = src.UnitOfAnalysis,
                PrimaryInvestigator = src.PrimaryInvestigator,
                Funding = src.Funding,
                Consultant = src.Consultant,
                ModeOfDataCollection = src.ModeOfDataCollection,
                Notes = src.Notes,
                Keywords = src.Keywords,
                AgreeToMakeThisQuestionnairePublic = src.AgreeToMakeThisQuestionnairePublic
            };
        }

        private static WB.Core.SharedKernels.Questionnaire.Documents.QuestionnaireMetaInfo? MapMetaInfoFromModel(Models.QuestionnaireMetaInfo? src)
        {
            if (src == null) return null;
            return new WB.Core.SharedKernels.Questionnaire.Documents.QuestionnaireMetaInfo
            {
                SubTitle = src.SubTitle,
                StudyType = src.StudyType,
                Version = src.Version,
                VersionNotes = src.VersionNotes,
                KindOfData = src.KindOfData,
                Country = src.Country,
                Year = src.Year,
                Language = src.Language,
                Coverage = src.Coverage,
                Universe = src.Universe,
                UnitOfAnalysis = src.UnitOfAnalysis,
                PrimaryInvestigator = src.PrimaryInvestigator,
                Funding = src.Funding,
                Consultant = src.Consultant,
                ModeOfDataCollection = src.ModeOfDataCollection,
                Notes = src.Notes,
                Keywords = src.Keywords,
                AgreeToMakeThisQuestionnairePublic = src.AgreeToMakeThisQuestionnairePublic
            };
        }

        private static Models.Macro MapMacro(Documents.Macro src) =>
            new Models.Macro { Name = src.Name, Description = src.Description, Content = src.Content };

        private static Documents.Macro MapMacroFromModel(Models.Macro src) =>
            new Documents.Macro { Name = src.Name, Description = src.Description, Content = src.Content };

        private static Models.LookupTable MapLookupTable(Documents.LookupTable src) =>
            new Models.LookupTable { TableName = src.TableName, FileName = src.FileName };

        private static Documents.LookupTable MapLookupTableFromModel(Models.LookupTable src) =>
            new Documents.LookupTable { TableName = src.TableName, FileName = src.FileName };

        private static Models.Attachment MapAttachment(Documents.Attachment src) =>
            new Models.Attachment { Name = src.Name, FileName = src.ContentId, ContentType = string.Empty };

        private static Documents.Attachment MapAttachmentFromModel(Models.Attachment src) =>
            new Documents.Attachment { AttachmentId = Guid.NewGuid(), Name = src.Name, ContentId = src.FileName };

        private static Models.Translation MapTranslation(Documents.Translation src) =>
            new Models.Translation { Id = src.Id, Name = src.Name };

        private static Documents.Translation MapTranslationFromModel(Models.Translation src) =>
            new Documents.Translation { Id = src.Id ?? Guid.NewGuid(), Name = src.Name };

        private static Models.Categories MapCategories(Documents.Categories src) =>
            new Models.Categories { Id = src.Id, Name = src.Name };

        private static Documents.Categories MapCategoriesFromModel(Models.Categories src) =>
            new Documents.Categories { Id = src.Id.HasValue ? src.Id.Value : Guid.NewGuid(), Name = src.Name };

        private static Models.CriticalRule MapCriticalRule(Documents.CriticalRule src) =>
            new Models.CriticalRule { Id = src.Id, Message = src.Message, Expression = src.Expression, Description = src.Description };

        private static Documents.CriticalRule MapCriticalRuleFromModel(Models.CriticalRule src) =>
            new Documents.CriticalRule
            {
                Id = src.Id.HasValue ? src.Id.Value : Guid.NewGuid(),
                Message = src.Message,
                Expression = src.Expression,
                Description = src.Description
            };

        private static Models.ValidationCondition MapValidationCondition(QuestionnaireEntities.ValidationCondition src) =>
            new Models.ValidationCondition { Expression = src.Expression, Message = src.Message, Severity = src.Severity };

        private static QuestionnaireEntities.ValidationCondition MapValidationConditionFromModel(Models.ValidationCondition src) =>
            new QuestionnaireEntities.ValidationCondition { Expression = src.Expression, Message = src.Message, Severity = src.Severity };

        private static Models.Answer MapAnswerToAnswer(Answer src) =>
            new Models.Answer
            {
                Text = src.AnswerText,
                Code = src.HasValue() ? (int)src.GetParsedValue() : (int?)null,
                ParentCode = src.GetParsedParentValue(),
                AttachmentName = src.AttachmentName
            };

        private static Models.SpecialValue MapAnswerToSpecialValue(Answer src) =>
            new Models.SpecialValue
            {
                Text = src.AnswerText,
                Code = (int)(src.AnswerCode ?? decimal.Parse(src.AnswerValue, NumberStyles.Number, CultureInfo.InvariantCulture)),
                AttachmentName = src.AttachmentName ?? string.Empty
            };

        private static Answer MapAnswerFromModel(Models.Answer src) =>
            new Answer
            {
                AnswerValue = src.Code.ToString() ?? string.Empty,
                ParentValue = src.ParentCode.ToString(),
                AnswerText = src.Text,
                AttachmentName = src.AttachmentName
            };

        private static Answer MapSpecialValueToAnswer(Models.SpecialValue src) =>
            new Answer
            {
                AnswerValue = src.Code.ToString() ?? string.Empty,
                AnswerText = src.Text,
                AttachmentName = src.AttachmentName
            };

        // ===================== Utility methods =====================

        private static string? GetVarName(Guid? id, Dictionary<Guid, string> idToVarMap)
        {
            if (!id.HasValue)
                return null;
            if (!idToVarMap.TryGetValue(id.Value, out var varName))
                return null;
            if (varName?.Trim().IsNullOrEmpty() ?? true)
                return null;
            return varName;
        }

        private static Guid GetIdOrGenerate(string? variableName, Guid? id, Dictionary<string, Guid> varToIdMap)
        {
            if (id.HasValue)
                return id.Value;
            if (variableName == null || variableName.Trim().IsNullOrEmpty())
                return Guid.NewGuid();
            return varToIdMap[variableName];
        }

        private static Guid? GetId(string? variableName, Dictionary<string, Guid> varToIdMap)
        {
            if (variableName == null || variableName.Trim().IsNullOrEmpty())
                return null;
            if (!varToIdMap.TryGetValue(variableName, out var id))
                return null;
            return id;
        }

        private static bool IsIdentified(IComposite composite) =>
            composite is IQuestion question && question.Featured;
    }
}
