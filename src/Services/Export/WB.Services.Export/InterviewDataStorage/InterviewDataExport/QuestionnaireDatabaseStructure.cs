using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure;

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
            var printIndex = tableIndex == 0 ? "" : $"__{tableIndex}";
            var questionnairePrefix = GenerateQuestionnairePrefix(questionnaire.VariableName, questionnaire.QuestionnaireId);
            var groupName = GenerateGroupName(@group);
            var tableName = $"{questionnairePrefix}{groupName}{printIndex}";
            var enablementTableName = $"{tableName}__e";
            var validityTableName = $"{tableName}__v";

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
            return "_" + (@group.VariableName ?? @group.PublicKey.FormatGuid());
        }

        protected string GenerateQuestionnairePrefix(string questionnaireVariable, QuestionnaireId questionnaireId)
        {
            var strings = questionnaireId.Id.Split('$');

            if (!string.IsNullOrEmpty(questionnaireVariable) && questionnaireVariable.Length < 22)
            {
                strings[0] = questionnaireVariable;
            }
            else
            {
                strings[0] = Convert.ToBase64String(Guid.Parse(strings[0]).ToByteArray())
                    .Substring(0, 22)
                    .Replace("/", "_")
                    .Replace("+", "-");
            }

            return string.Join("$", strings);
        }

        private Dictionary<Guid, List<QuestionnaireLevelDatabaseTable>> GetDatabaseTableMap(QuestionnaireDocument questionnaire)
        {
            var databaseTableMap = new Dictionary<Guid, List<QuestionnaireLevelDatabaseTable>>(); ;

            var allGroups = (questionnaire as IQuestionnaireEntity).TreeToEnumerable(composite => composite.Children)
                .Where(entity => entity is Group)
                .Cast<Group>();

            foreach (var @group in allGroups)
            {
                Group parent = @group;
                while (true)
                {
                    if (parent is Group roster && roster.IsRoster || parent is QuestionnaireDocument)
                        break;

                    parent = parent.GetParent() as Group;
                }

                if (!databaseTableMap.ContainsKey(parent.PublicKey))
                {
                    databaseTableMap.Add(parent.PublicKey, new List<QuestionnaireLevelDatabaseTable>()
                    {
                        CreateTable(questionnaire, parent, 0)
                    });
                }

                var levelTables = databaseTableMap[parent.PublicKey];
                var lastTable = levelTables.Last();
                if (!lastTable.CanAddChildren(@group))
                {
                    var newTable = CreateTable(questionnaire, parent, levelTables.Count);
                    levelTables.Add(newTable);
                    lastTable = newTable;
                }

                lastTable.AddChildren(group);
            }

            return databaseTableMap;
        }
    }
}
