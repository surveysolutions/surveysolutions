using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models
{
    public class LevelModel
    {
        private static readonly List<QuestionType> ExcludedQuestionTypes = new List<QuestionType>
        {
            QuestionType.Area
        };

        public LevelModel(string variable, RosterScope rosterScope, string className)
        {
            Variable = variable;
            RosterScope = rosterScope;
            ClassName = className;
        }
       
        public string Variable {get; }
        public string ClassName { get; }
        public RosterScope RosterScope { get; }

        public List<QuestionModel> Questions { get; } = new List<QuestionModel>();
        public List<RosterModel> Rosters { get; } = new List<RosterModel>();
        public List<VariableModel> Variables { get; } = new List<VariableModel>();

        public void Init(ReadOnlyQuestionnaireDocument questionnaire, Dictionary<RosterScope, string> levelClassNames, IQuestionTypeToCSharpTypeMapper questionTypeMapper)
        {
            this.CreateQuestionsForCurrentAndParentLevels(questionnaire, questionTypeMapper);

            this.CreateVariablesForCurrentAndParentLevels(questionnaire, questionTypeMapper);

            this.CreateRostersForCurrentAndParentLevels(questionnaire, levelClassNames);
        }

        private void CreateRostersForCurrentAndParentLevels(ReadOnlyQuestionnaireDocument questionnaire, Dictionary<RosterScope, string> levelClassNames)
        {
            foreach (var roster in questionnaire.Find<IGroup>(x => x.IsRoster))
            {
                var rosterScope = questionnaire.GetRosterScope(roster);
                var isNestedRoster = rosterScope.Length > 1;
                if (isNestedRoster && !rosterScope.IsSameOrParentScopeFor(this.RosterScope))
                {
                    if (!rosterScope.IsChildScopeFor(this.RosterScope, 1))
                        continue;
                }

                var className = levelClassNames[rosterScope];
                this.Rosters.Add(new RosterModel
                {
                    Variable = questionnaire.GetVariable(roster),
                    RosterScope = rosterScope,
                    ClassName = className
                });
            }
        }

        private void CreateVariablesForCurrentAndParentLevels(ReadOnlyQuestionnaireDocument questionnaire,
            IQuestionTypeToCSharpTypeMapper questionTypeMapper)
        {
            foreach (var variable in questionnaire.Find<IVariable>())
            {
                var rosterScope = questionnaire.GetRosterScope(variable);
                if (!rosterScope.IsSameOrParentScopeFor(this.RosterScope)) continue;

                this.Variables.Add(new VariableModel
                {
                    Id = variable.PublicKey,
                    Variable = questionnaire.GetVariable(variable),
                    TypeName = questionTypeMapper.GetVariableType(variable.Type),
                    RosterScope = rosterScope
                });
            }
        }

        private void CreateQuestionsForCurrentAndParentLevels(ReadOnlyQuestionnaireDocument questionnaire,
            IQuestionTypeToCSharpTypeMapper questionTypeMapper)
        {
            foreach (var question in questionnaire.Find<IQuestion>().Where(x => !ExcludedQuestionTypes.Contains(x.QuestionType)))
            {
                var rosterScope = questionnaire.GetRosterScope(question);
                if (!rosterScope.IsSameOrParentScopeFor(this.RosterScope)) continue;

                this.Questions.Add(new QuestionModel
                {
                    Id = question.PublicKey,
                    Variable = questionnaire.GetVariable(question),
                    TypeName = questionTypeMapper.GetQuestionType(question, questionnaire),
                    RosterScope = rosterScope
                });
            }
        }
    }
}