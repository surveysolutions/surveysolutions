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
    /// Exports actual display order of questionnaire items for every questionnaire level (section, sub-section or roster)
    /// that contains at least one item marked with "Randomize order".
    /// </summary>
    public class InterviewItemsOrderExporter : IInterviewItemsOrderExporter
    {
        public const string FileNameSuffix = "__order";
        public const string RootLevelName = "sections";

        public const string ItemTypeQuestion = "Question";
        public const string ItemTypeStaticText = "StaticText";
        public const string ItemTypeSection = "Section";
        public const string ItemTypeRoster = "Roster";
        public const string ItemTypeRosterInstance = "RosterInstance";

        private readonly ICsvWriter csvWriter;

        public static readonly DoExportFileHeader[] FileColumns =
        {
            CommonHeaderItems.InterviewKey,
            CommonHeaderItems.InterviewId,
            new DoExportFileHeader("parent__id", "Id of the parent questionnaire item (section, sub-section or roster)", ExportValueType.String),
            new DoExportFileHeader("roster__instance", "Roster instance path of the item, roster codes from top to bottom separated with '/'", ExportValueType.String),
            new DoExportFileHeader("item__id", "Id of the questionnaire item", ExportValueType.String),
            new DoExportFileHeader("item__type", "Type of the questionnaire item: Question, StaticText, Section, Roster or RosterInstance", ExportValueType.String),
            new DoExportFileHeader("item__order", "Displayed position of the item within the parent, starting from 1", ExportValueType.NumericInt)
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
                .Where(group => RandomizedItemsOrder.HasRandomizedChildren(questionnaire, group));

            foreach (var group in groups)
            {
                var levelName = group is QuestionnaireDocument
                    ? RootLevelName
                    : string.IsNullOrWhiteSpace(group.VariableName) ? group.PublicKey.FormatGuid() : group.VariableName;

                if (!usedNames.Add(levelName))
                {
                    levelName = group.PublicKey.FormatGuid();
                    usedNames.Add(levelName);
                }

                result.Add(group.PublicKey, levelName + FileNameSuffix);
            }

            return result;
        }

        public void WriteHeaders(QuestionnaireDocument questionnaire, string basePath, string dataFileExtension)
        {
            var header = FileColumns.Select(x => x.Title).ToArray();

            foreach (var fileName in GetFileNames(questionnaire).Values)
            {
                var filePath = Path.Combine(basePath, $"{fileName}.{dataFileExtension}");
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

                foreach (var parentRosterVector in state.GetVisibleContexts(parent))
                {
                    var position = 0;
                    var randomizedRosters = new List<Group>();

                    foreach (var child in RandomizedItemsOrder.GetChildrenOrder(questionnaire, seed, parent, parentRosterVector))
                    {
                        var itemType = state.GetVisibleItemType(child, parentRosterVector);
                        if (itemType == null) continue;

                        position++;
                        rows.Add(CreateRow(interviewKey, formattedInterviewId, parent.PublicKey, parentRosterVector,
                            child.PublicKey, itemType, position));

                        if (child is Group { IsRoster: true } roster && RandomizedItemsOrder.IsOrderRandomized(questionnaire, roster))
                            randomizedRosters.Add(roster);
                    }

                    foreach (var roster in randomizedRosters)
                    {
                        var instances = state.GetVisibleRosterInstances(roster, parentRosterVector)
                            .Select(vector => new { vector, key = RandomizedItemsOrder.GetRosterInstanceSortKey(seed, roster.PublicKey, vector) })
                            .OrderBy(x => x.key)
                            .ThenBy(x => x.vector.Last())
                            .ToList();

                        for (int i = 0; i < instances.Count; i++)
                        {
                            rows.Add(CreateRow(interviewKey, formattedInterviewId, roster.PublicKey, instances[i].vector,
                                roster.PublicKey, ItemTypeRosterInstance, i + 1));
                        }
                    }
                }

                result[level.Value] = rows;
            }

            return result;
        }

        private static string[] CreateRow(string interviewKey, string interviewId, Guid parentId, RosterVector rosterVector,
            Guid itemId, string itemType, int position)
        {
            return new[]
            {
                interviewKey,
                interviewId,
                parentId.FormatGuid(),
                string.Join("/", rosterVector.Coordinates.Select(x => x.ToString(CultureInfo.InvariantCulture))),
                itemId.FormatGuid(),
                itemType,
                position.ToString(CultureInfo.InvariantCulture)
            };
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
