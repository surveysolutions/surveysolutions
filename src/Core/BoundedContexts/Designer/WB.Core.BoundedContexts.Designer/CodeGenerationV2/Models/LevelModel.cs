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
        public LevelModel(string variable, RosterScope rosterScope, string className)
        {
            Variable = variable;
            RosterScope = rosterScope;
            ClassName = className;
        }
       
        public string Variable {get; }
        public string ClassName { get; }
        public bool HasCriticalityConditions { get; private set; }
        public RosterScope RosterScope { get; }

        public List<QuestionModel> Questions { get; } = new List<QuestionModel>();
        public List<RosterModel> Rosters { get; } = new List<RosterModel>();
        public List<VariableModel> Variables { get; } = new List<VariableModel>();
        public List<SectionModel> Sections { get; } = new List<SectionModel>();

        public void Init(ReadOnlyQuestionnaireDocument questionnaire, Dictionary<RosterScope, string> levelClassNames, IQuestionTypeToCSharpTypeMapper questionTypeMapper)
        {
            if (RosterScope.Length == 0)
                this.HasCriticalityConditions = questionnaire.CriticalityConditions.Any();
            
            this.CreateQuestionsForCurrentAndParentLevels(questionnaire, questionTypeMapper);

            this.CreateVariablesForCurrentAndParentLevels(questionnaire, questionTypeMapper);

            this.CreateRostersForCurrentAndParentLevels(questionnaire, levelClassNames);

            this.CreateSectionsForCurrentAndParentLevels(questionnaire);
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
                (
                    variable : questionnaire.GetVariable(roster),
                    rosterScope : rosterScope,
                    className : className
                ));
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
                (
                    id : variable.PublicKey,
                    variable : questionnaire.GetVariable(variable),
                    typeName : questionTypeMapper.GetVariableType(variable.Type),
                    rosterScope : rosterScope
                ));
            }
        }

        private void CreateQuestionsForCurrentAndParentLevels(ReadOnlyQuestionnaireDocument questionnaire,
            IQuestionTypeToCSharpTypeMapper questionTypeMapper)
        {
            foreach (var question in questionnaire.Find<IQuestion>())
            {
                var rosterScope = questionnaire.GetRosterScope(question);
                if (!rosterScope.IsSameOrParentScopeFor(this.RosterScope)) continue;

                this.Questions.Add(new QuestionModel
                (
                    id : question.PublicKey,
                    variable : questionnaire.GetVariable(question),
                    typeName : questionTypeMapper.GetQuestionType(question, questionnaire),
                    rosterScope : rosterScope
                ));
            }
        }


        private void CreateSectionsForCurrentAndParentLevels(ReadOnlyQuestionnaireDocument questionnaire)
        {
            foreach (var section in questionnaire.Find<IGroup>(x => !x.IsRoster))
            {
                var rosterScope = questionnaire.GetRosterScope(section);
                if (!rosterScope.IsSameOrParentScopeFor(this.RosterScope)) continue;

                this.Sections.Add(new SectionModel
                (
                    variable : questionnaire.GetVariable(section),
                    rosterScope : rosterScope
                ));
            }
        }
    }
}
