using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    public class CodeGenerationModel
    {
        public CodeGenerationModel()
        {
            this.AllQuestions = new List<QuestionModel>();
            this.AllLevels = new List<LevelModel>();
        }
        public Guid Id { set; get; }
        public List<QuestionModel> AllQuestions { set; get; }
        public List<LevelModel> AllLevels { set; get; }

        // change it later
        public List<RosterModel> AllRosters => AllLevels
            .SelectMany(x => x.Rosters)
            .GroupBy(x => x.Variable)
            .Select(x => x.First())
            .ToList();

        public List<Guid> ConditionsPlayOrder { get; set; }

        public string[] AdditionalInterfaces { get; set; }
        public string[] Namespaces { get; set; }

        public List<LookupTableTemplateModel> LookupTables { get; set; }
        public string ClassName { get; set; }

        public QuestionModel GetQuestionById(Guid questionId)
        {
            return AllQuestions.FirstOrDefault(x => x.Id == questionId);
        }

        public LevelModel GetLevelByVariable(string variable)
        {
            return AllRosters.FirstOrDefault(x => x.Variable == variable)?.Level;
        }
    }
}