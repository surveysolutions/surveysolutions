using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class QuestionnaireLevelDatabaseTable
    {
        private const int ColumnsLimit = 1400;

        public Guid Id { get; set; }
        public bool IsRoster { get; set; }
        public string TableName { get; set; }
        public string EnablementTableName { get; set; }
        public string ValidityTableName { get; set; }
        public List<IQuestionnaireEntity> DataColumns { get; set; } = new List<IQuestionnaireEntity>();
        public List<IQuestionnaireEntity> EnablementColumns { get; set; } = new List<IQuestionnaireEntity>();
        public List<IQuestionnaireEntity> ValidityColumns { get; set; } = new List<IQuestionnaireEntity>();
        public List<IQuestionnaireEntity> Entities { get; set; } = new List<IQuestionnaireEntity>();

        public bool CanAddChildren(Group group)
        {
            var questionsCount = group.Children.Count(entity => entity is Question);
            var variablesCount = group.Children.Count(entity => entity is Variable);
            var staticTextCount = group.Children.Count(entity => entity is StaticText);
            var groupsCount = group.Children.Count(entity => entity is Group roster && !roster.IsRoster);
            if (questionsCount + variablesCount + DataColumns.Count > ColumnsLimit
                || questionsCount + variablesCount + staticTextCount + groupsCount + EnablementColumns.Count > ColumnsLimit
                || questionsCount + staticTextCount + ValidityColumns.Count > ColumnsLimit)
                return false;

            return true;
        }

        public void AddChildren(Group group)
        {
            if (group.IsRoster || group is QuestionnaireDocument)
            {
                EnablementColumns.Add(group);
                Entities.Add(group);
            }

            foreach (var child in @group.Children)
            {
                switch (child)
                {
                    case Question question:
                        DataColumns.Add(question);
                        EnablementColumns.Add(question);
                        ValidityColumns.Add(question);
                        Entities.Add(child);
                        break;
                    case Variable variable:
                        DataColumns.Add(variable);
                        EnablementColumns.Add(variable);
                        Entities.Add(child);
                        break;
                    case StaticText staticText:
                        EnablementColumns.Add(staticText);
                        ValidityColumns.Add(staticText);
                        Entities.Add(child);
                        break;
                    case Group notRoster when  !notRoster.IsRoster:
                        EnablementColumns.Add(notRoster);
                        Entities.Add(child);
                        break;
                }
            }
        }
    }

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
        public bool DoesSupportDataTable(Guid entityId) => entityTableMap[entityId] != null;
        public bool DoesSupportEnablementTable(Guid entityId) => entityTableMap[entityId] != null;
        public bool DoesSupportValidityTable(Guid entityId) => entityTableMap[entityId] != null;

        public IEnumerable<QuestionnaireLevelDatabaseTable> GetAllLevelTables() => tables.SelectMany(level => level.Value);
        public IEnumerable<QuestionnaireLevelDatabaseTable> GetLevelTables(Guid entityId) => tables[entityId];

        private QuestionnaireLevelDatabaseTable CreateTable(QuestionnaireDocument questionnaire, Group @group, int tableIndex)
        {
            var printIndex = tableIndex == 0 ? "" : $"__{tableIndex}";
            var tableName = $"{CompressQuestionnaireId(questionnaire.QuestionnaireId)}_{@group.VariableName ?? @group.PublicKey.FormatGuid()}{printIndex}";
            var enablementTableName = $"{tableName}_e";
            var validityTableName = $"{tableName}_v";

            return new QuestionnaireLevelDatabaseTable()
            {
                Id = group.PublicKey,
                TableName = tableName,
                EnablementTableName = enablementTableName,
                ValidityTableName = validityTableName,
                IsRoster = group.IsRoster
            };
        }

        protected string CompressQuestionnaireId(QuestionnaireId questionnaireId)
        {
            var strings = questionnaireId.Id.Split('$');
            strings[0] = Convert.ToBase64String(Guid.Parse(strings[0]).ToByteArray())
                .Substring(0, 22)
                .Replace("/", "_")
                .Replace("+", "-");
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
