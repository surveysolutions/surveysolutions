using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class QuestionnaireLevelDatabaseTable
    {
        private const int ColumnsLimit = 1400;

        public Guid Id { get; set; }
        public bool IsRoster { get; set; }
        public string TableName { get; set; } = String.Empty;
        public string EnablementTableName { get; set; } = String.Empty;
        public string ValidityTableName { get; set; } = String.Empty;
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
            if (group.Children.Count() > ColumnsLimit)
                throw new ArgumentException($"Group {group.PublicKey} ({group.Children.Count()} children) fail to insert in table with {Entities.Count} entities. Max allowed {ColumnsLimit}");

            Entities.Add(group);

            if (!(group is QuestionnaireDocument))
            {
                EnablementColumns.Add(group);
            }

            foreach (var child in @group.Children)
            {
                if (!child.IsExportable) continue;

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
                }
            }
        }
    }
}
