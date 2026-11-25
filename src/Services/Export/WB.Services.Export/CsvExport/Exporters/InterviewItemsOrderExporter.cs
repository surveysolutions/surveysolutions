using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.CsvExport.Exporters
{
    /// <summary>
    /// Exports actual display order of questionnaire items marked with "Randomize order" for every questionnaire level
    /// (section, sub-section or roster) that contains such items. Only members of randomized groups are exported,
    /// items that are not randomized are skipped. Positions are numbered from 1 within every randomized group.
    /// Rows are identified with the same key columns as rows of the data file of the corresponding roster level.
    /// Display order of instances of a randomized roster is exported into the file of that roster.
    /// </summary>
    public class InterviewItemsOrderExporter : IInterviewItemsOrderExporter
    {
        public const string FileNameSuffix = "__order";

        public const string ItemTypeQuestion = "Question";
        public const string ItemTypeStaticText = "StaticText";
        public const string ItemTypeSection = "Section";
        public const string ItemTypeRoster = "Roster";
        public const string ItemTypeRosterInstance = "RosterInstance";

        private readonly ICsvWriter csvWriter;

        public static readonly DoExportFileHeader[] ItemColumns =
        {
            new DoExportFileHeader("randomization__group", "Number of the randomized group (consecutive items marked with 'Randomize order') within the parent, starting from 1. Empty for roster instances", ExportValueType.NumericInt),
            new DoExportFileHeader("variable", "Variable name of the questionnaire item or its id if item has no variable name", ExportValueType.String),
            new DoExportFileHeader("type", "Type of the questionnaire item: Question, StaticText, Section, Roster or RosterInstance", ExportValueType.String),
            new DoExportFileHeader("order", "Displayed position of the item within its randomized group or of the roster instance among instances of the roster, starting from 1", ExportValueType.NumericInt)
        };

        public InterviewItemsOrderExporter(ICsvWriter csvWriter)
        {
            this.csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
        }

        public Dictionary<Guid, string> GetFileNames(QuestionnaireDocument questionnaire)
        {
            var result = new Dictionary<Guid, string>();
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var groups = new IQuestionnaireEntity[] { questionnaire }.TreeToEnumerable(x => x.Children)
                .OfType<Group>()
                .Where(group => RandomizedItemsOrder.HasRandomizedChildren(questionnaire, group) || IsRandomizedRoster(questionnaire, group));

            foreach (var group in groups)
            {
                var levelName = group is QuestionnaireDocument
                    ? GetQuestionnaireLevelName(questionnaire)
                    : group.VariableName;

                if (string.IsNullOrWhiteSpace(levelName) || !usedNames.Add(levelName))
                {
                    levelName = group.PublicKey.FormatGuid();
                    usedNames.Add(levelName);
                }

                result.Add(group.PublicKey, levelName + FileNameSuffix);
            }

            return result;
        }

        private static string? GetQuestionnaireLevelName(QuestionnaireDocument questionnaire)
            => string.IsNullOrWhiteSpace(questionnaire.VariableName)
                ? questionnaire.Title.MakeStataCompatibleFileName()
                : questionnaire.VariableName;

        public string[] GetHeader(QuestionnaireExportStructure exportStructure, QuestionnaireDocument questionnaire, Guid groupId)
        {
            // questionnaire document itself is not stored in the entities cache
            var rosterScope = groupId == questionnaire.PublicKey
                ? new ValueVector<Guid>()
                : new ValueVector<Guid>(questionnaire.GetRosterSizeSourcesForEntity(groupId));

            var header = new List<string> { CommonHeaderItems.InterviewKey.Title };

            if (rosterScope.Length == 0)
            {
                header.Add(CommonHeaderItems.InterviewId.Title);
            }
            else
            {
                header.AddRange(exportStructure.GetAllParentColumnNamesForLevel(rosterScope));
                header.Add(exportStructure.HeaderToLevelMap.TryGetValue(rosterScope, out var level)
                    ? level.LevelIdColumnName
                    : string.Format(ServiceColumns.IdSuffixFormat, $"{ServiceColumns.ParentId}{rosterScope.Length}"));
            }

            header.AddRange(ItemColumns.Select(x => x.Title));
            return header.ToArray();
        }

        public void WriteHeaders(QuestionnaireExportStructure exportStructure, QuestionnaireDocument questionnaire, string basePath, string dataFileExtension)
        {
            foreach (var file in GetFileNames(questionnaire))
            {
                var filePath = Path.Combine(basePath, $"{file.Value}.{dataFileExtension}");
                var header = GetHeader(exportStructure, questionnaire, file.Key);
                this.csvWriter.WriteData(filePath, new[] { header }, ExportFileSettings.DataFileSeparator.ToString());
            }
        }

        public Dictionary<string, List<string[]>> Export(QuestionnaireDocument questionnaire,
            List<InterviewEntity> interviewEntities, Guid interviewId, string interviewKey)
        {
            var result = new Dictionary<string, List<string[]>>();
            var fileNames = GetFileNames(questionnaire);
            if (fileNames.Count == 0)
                return result;

            var state = new InterviewState(questionnaire, interviewEntities);
            var seed = interviewId.GetRandomDouble();
            var formattedInterviewId = interviewId.FormatGuid();

            foreach (var level in fileNames)
            {
                var parent = level.Key == questionnaire.PublicKey
                    ? questionnaire
                    : questionnaire.Find<Group>(level.Key);
                if (parent == null) continue;

                var rows = new List<string[]>();
                var contexts = state.GetVisibleContexts(parent).ToList();
                var hasRandomizedChildren = RandomizedItemsOrder.HasRandomizedChildren(questionnaire, parent);
                var instancePositions = IsRandomizedRoster(questionnaire, parent)
                    ? GetRosterInstancePositions(seed, parent, contexts)
                    : null;

                foreach (var parentRosterVector in contexts)
                {
                    if (instancePositions != null && instancePositions.TryGetValue(parentRosterVector, out var instancePosition))
                    {
                        rows.Add(CreateRow(interviewKey, formattedInterviewId, parentRosterVector,
                            null, GetVariable(parent), ItemTypeRosterInstance, instancePosition));
                    }

                    if (!hasRandomizedChildren)
                        continue;

                    var children = parent.Children.ToArray();
                    var orderedChildren = RandomizedItemsOrder.GetChildrenOrder(questionnaire, seed, parent, parentRosterVector);
                    var groupNumber = 0;
                    var position = 0;

                    // randomized items are shuffled within the slots of their group, so original position
                    // of the slot tells whether item belongs to a randomized group and where the group starts
                    for (int i = 0; i < children.Length; i++)
                    {
                        if (!RandomizedItemsOrder.IsOrderRandomized(questionnaire, children[i]))
                            continue;

                        if (i == 0 || !RandomizedItemsOrder.IsOrderRandomized(questionnaire, children[i - 1]))
                        {
                            groupNumber++;
                            position = 0;
                        }

                        var child = orderedChildren[i];
                        var itemType = state.GetVisibleItemType(child, parentRosterVector);
                        if (itemType == null) continue;

                        position++;
                        rows.Add(CreateRow(interviewKey, formattedInterviewId, parentRosterVector,
                            groupNumber, GetVariable(child), itemType, position));
                    }
                }

                result[level.Value] = rows;
            }

            return result;
        }

        private static bool IsRandomizedRoster(QuestionnaireDocument questionnaire, Group group)
            => group.IsRoster && RandomizedItemsOrder.IsOrderRandomized(questionnaire, group);

        private static Dictionary<RosterVector, int> GetRosterInstancePositions(double seed, Group roster, IEnumerable<RosterVector> instances)
        {
            var result = new Dictionary<RosterVector, int>();

            foreach (var siblings in instances.GroupBy(vector => vector.Take(vector.Length - 1)))
            {
                var ordered = siblings
                    .Select(vector => new { vector, key = RandomizedItemsOrder.GetRosterInstanceSortKey(seed, roster.PublicKey, vector) })
                    .OrderBy(x => x.key)
                    .ThenBy(x => x.vector.Last())
                    .ToList();

                for (int i = 0; i < ordered.Count; i++)
                    result[ordered[i].vector] = i + 1;
            }

            return result;
        }

        private static string GetVariable(IQuestionnaireEntity entity)
        {
            var variable = entity switch
            {
                Question question => question.VariableName,
                Group group => group.VariableName,
                _ => null
            };

            return string.IsNullOrWhiteSpace(variable) ? entity.PublicKey.FormatGuid() : variable;
        }

        private static string[] CreateRow(string interviewKey, string interviewId, RosterVector rosterVector,
            int? groupNumber, string variable, string itemType, int position)
        {
            var row = new List<string> { interviewKey, interviewId };
            row.AddRange(rosterVector.Coordinates.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            row.Add(groupNumber?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
            row.Add(variable);
            row.Add(itemType);
            row.Add(position.ToString(CultureInfo.InvariantCulture));
            return row.ToArray();
        }

        private class InterviewState
        {
            private readonly QuestionnaireDocument questionnaire;
            private readonly Dictionary<Identity, InterviewEntity> entities = new Dictionary<Identity, InterviewEntity>();
            private readonly HashSet<Identity> groupsWithEnabledDescendants = new HashSet<Identity>();
            private readonly Dictionary<Guid, List<RosterVector>> groupContexts = new Dictionary<Guid, List<RosterVector>>();
            private readonly Dictionary<Guid, int> rosterLevelDepth = new Dictionary<Guid, int>();

            public InterviewState(QuestionnaireDocument questionnaire, List<InterviewEntity> interviewEntities)
            {
                this.questionnaire = questionnaire;

                foreach (var entity in interviewEntities)
                {
                    entities[entity.Identity] = entity;

                    if (entity.EntityType == EntityType.Section)
                    {
                        if (!groupContexts.TryGetValue(entity.Identity.Id, out var contexts))
                            groupContexts[entity.Identity.Id] = contexts = new List<RosterVector>();
                        contexts.Add(entity.Identity.RosterVector);
                    }

                    if (!entity.IsEnabled) continue;

                    var questionnaireEntity = questionnaire.Find<IQuestionnaireEntity>(entity.Identity.Id);
                    var ancestor = questionnaireEntity?.GetParent();
                    while (ancestor is Group group && !(group is QuestionnaireDocument))
                    {
                        var depth = GetRosterLevelDepth(group);
                        if (depth > entity.Identity.RosterVector.Length) break;

                        var identity = new Identity(group.PublicKey, entity.Identity.RosterVector.Take(depth));
                        if (groupsWithEnabledDescendants.Add(identity) && !entities.ContainsKey(identity))
                        {
                            if (!groupContexts.TryGetValue(group.PublicKey, out var contexts))
                                groupContexts[group.PublicKey] = contexts = new List<RosterVector>();
                            contexts.Add(identity.RosterVector);
                        }

                        ancestor = group.GetParent();
                    }
                }
            }

            public IEnumerable<RosterVector> GetVisibleContexts(Group group)
            {
                if (group is QuestionnaireDocument)
                    return new[] { RosterVector.Empty };

                if (!groupContexts.TryGetValue(group.PublicKey, out var contexts))
                    return Enumerable.Empty<RosterVector>();

                return contexts
                    .Distinct()
                    .Where(vector => IsGroupVisible(group, vector))
                    .OrderBy(vector => vector, RosterVectorComparer.Instance)
                    .ToList();
            }

            public string? GetVisibleItemType(IQuestionnaireEntity child, RosterVector parentRosterVector)
            {
                switch (child)
                {
                    case Question question:
                        if (question.QuestionScope != QuestionScope.Interviewer || question.Featured)
                            return null;
                        return IsEnabled(question.PublicKey, parentRosterVector) ? ItemTypeQuestion : null;
                    case StaticText staticText:
                        return IsEnabled(staticText.PublicKey, parentRosterVector) ? ItemTypeStaticText : null;
                    case Group { IsRoster: true } roster:
                        return GetVisibleRosterInstances(roster, parentRosterVector).Any() ? ItemTypeRoster : null;
                    case Group group:
                        return IsGroupVisible(group, parentRosterVector) ? ItemTypeSection : null;
                    default:
                        return null;
                }
            }

            public IEnumerable<RosterVector> GetVisibleRosterInstances(Group roster, RosterVector parentRosterVector)
            {
                if (!groupContexts.TryGetValue(roster.PublicKey, out var contexts))
                    return Enumerable.Empty<RosterVector>();

                return contexts
                    .Distinct()
                    .Where(vector => vector.Length == parentRosterVector.Length + 1
                                     && vector.Take(parentRosterVector.Length).Identical(parentRosterVector)
                                     && IsGroupVisible(roster, vector))
                    .OrderBy(vector => vector, RosterVectorComparer.Instance)
                    .ToList();
            }

            private bool IsEnabled(Guid entityId, RosterVector rosterVector)
                => entities.TryGetValue(new Identity(entityId, rosterVector), out var entity) && entity.IsEnabled;

            private bool IsGroupVisible(Group group, RosterVector rosterVector)
            {
                var identity = new Identity(group.PublicKey, rosterVector);
                if (entities.TryGetValue(identity, out var entity))
                    return entity.IsEnabled;

                // groups without own enablement data (containing only sub-sections or rosters) are visible
                // when at least one of their descendants is enabled
                return groupsWithEnabledDescendants.Contains(identity);
            }

            private int GetRosterLevelDepth(Group group)
            {
                if (rosterLevelDepth.TryGetValue(group.PublicKey, out var depth))
                    return depth;

                depth = 0;
                IQuestionnaireEntity? current = group;
                while (current is Group currentGroup && !(current is QuestionnaireDocument))
                {
                    if (currentGroup.IsRoster) depth++;
                    current = current.GetParent();
                }

                rosterLevelDepth[group.PublicKey] = depth;
                return depth;
            }
        }

        private class RosterVectorComparer : IComparer<RosterVector>
        {
            public static readonly RosterVectorComparer Instance = new RosterVectorComparer();

            public int Compare(RosterVector? x, RosterVector? y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                for (int i = 0; i < Math.Min(x.Length, y.Length); i++)
                {
                    var result = x[i].CompareTo(y[i]);
                    if (result != 0) return result;
                }

                return x.Length.CompareTo(y.Length);
            }
        }
    }
}
