using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class QuestionnaireDatabaseStructure
    {
        private readonly Dictionary<Guid, List<QuestionnaireLevelDatabaseTable>> tables;
        private readonly Dictionary<Guid, QuestionnaireLevelDatabaseTable> entityTableMap;

        public QuestionnaireDatabaseStructure(QuestionnaireDocument questionnaireDocument)
        {
            tables = GetDatabaseTableMap(questionnaireDocument);
            entityTableMap = GetEntityTableMap(tables);
        }

        private Dictionary<Guid, QuestionnaireLevelDatabaseTable> GetEntityTableMap(Dictionary<Guid, List<QuestionnaireLevelDatabaseTable>> levelTables)
        {
            var map = new Dictionary<Guid, QuestionnaireLevelDatabaseTable>();
            var splitedTables = levelTables.SelectMany(level => level.Value);

            foreach (var table in splitedTables)
            {
                foreach (var entity in table.Entities)
                {
                    map.Add(entity.PublicKey, table);
                }
            }

            return map;
        }

        public string GetDataTableName(Guid entityId) => entityTableMap[entityId].TableName;
        public string GetEnablementDataTableName(Guid entityId) => entityTableMap[entityId].EnablementTableName;
        public string GetValidityDataTableName(Guid entityId) => entityTableMap[entityId].ValidityTableName;

        public IEnumerable<QuestionnaireLevelDatabaseTable> GetAllLevelTables() => tables.SelectMany(level => level.Value);
        public IEnumerable<QuestionnaireLevelDatabaseTable> GetLevelTables(Guid entityId) => tables[entityId];

        private QuestionnaireLevelDatabaseTable CreateTable(QuestionnaireDocument questionnaire, Group @group, int tableIndex)
        {
            var printIndex = tableIndex == 0 ? "" : $"${tableIndex}";
            var questionnairePrefix = GenerateQuestionnairePrefix(questionnaire.VariableName, questionnaire.QuestionnaireId);
            var groupName = GenerateGroupName(@group);
            var tableName = $"{questionnairePrefix}{groupName}{printIndex}";
            var enablementTableName = $"{tableName}-e";
            var validityTableName = $"{tableName}-v";

            if (tableName.Length > 64 || enablementTableName.Length > 64 || validityTableName.Length > 64)
                throw new ArgumentException($"Table name can't be more 64 chars: data-{tableName}, enablement-{enablementTableName}, validity-{validityTableName}");

            return new QuestionnaireLevelDatabaseTable()
            {
                Id = group.PublicKey,
                TableName = tableName,
                EnablementTableName = enablementTableName,
                ValidityTableName = validityTableName,
                IsRoster = group.IsRoster
            };
        }

        private string GenerateGroupName(Group @group)
        {
            if (group is QuestionnaireDocument)
                return string.Empty;
            if (!string.IsNullOrEmpty(group.VariableName) && group.VariableName.Length <= 22)
                return "_" + group.VariableName;

            return "_" + CompressGuidTo22Chars(@group.PublicKey);
        }

        protected string GenerateQuestionnairePrefix(string questionnaireVariable, QuestionnaireIdentity questionnaireId)
        {
            var strings = new string[2]
            {
                questionnaireId.Id.FormatGuid(),
                questionnaireId.Version.ToString()
            };

            if (!string.IsNullOrEmpty(questionnaireVariable) && questionnaireVariable.Length <= 22)
            {
                strings[0] = questionnaireVariable;
            }
            else
            {
                strings[0] = CompressGuidTo22Chars(Guid.Parse(strings[0]));
            }

            return string.Join("$", strings);
        }

        private static string CompressGuidTo22Chars(Guid guid)
        {
            return Convert.ToBase64String(guid.ToByteArray())
                .Substring(0, 22)
                .Replace("/", "_")
                .Replace("+", "-");
        }

        private Dictionary<Guid, List<QuestionnaireLevelDatabaseTable>> GetDatabaseTableMap(QuestionnaireDocument questionnaire)
        {
            var databaseTableMap = new Dictionary<Guid, List<QuestionnaireLevelDatabaseTable>>(); ;

            var allGroups = (questionnaire as IQuestionnaireEntity).TreeToEnumerable(composite => composite.Children)
                .Where(entity => entity is Group)
                .Cast<Group>();

            foreach (var @group in allGroups)
            {
                Group? currentLevel = @group;
                while (currentLevel != null)
                {
                    if (currentLevel is Group roster && roster.IsRoster || currentLevel is QuestionnaireDocument)
                        break;

                    currentLevel = currentLevel.GetParent() as Group;
                }

                if(currentLevel == null)
                    throw new InvalidOperationException("Invalid structure. Parent was not found.");

                if (!databaseTableMap.ContainsKey(currentLevel.PublicKey))
                {
                    databaseTableMap.Add(currentLevel.PublicKey, new List<QuestionnaireLevelDatabaseTable>()
                    {
                        CreateTable(questionnaire, currentLevel, 0)
                    });
                }

                var levelTables = databaseTableMap[currentLevel.PublicKey];
                var lastTable = levelTables.Last();
                if (!lastTable.CanAddChildren(@group))
                {
                    var newTable = CreateTable(questionnaire, currentLevel, levelTables.Count);
                    levelTables.Add(newTable);
                    lastTable = newTable;
                }

                lastTable.AddChildren(group);
            }

            return databaseTableMap;
        }
    }
}
